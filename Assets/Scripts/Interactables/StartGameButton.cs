using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StartGameButton : MonoBehaviourPun, IInteractable
{
    [SerializeField] private GameTagManager gameTagManager;
    [SerializeField] private Transform killerSpawnPoint;

    public void Interact()
    {
        // Cualquier jugador puede interactuar, enviamos RPC a todos
        photonView.RPC("RPC_StartGame", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartGame()
    {
        // Solo el Master Client asigna los tags
        if (PhotonNetwork.IsMasterClient)
        {
            gameTagManager.AssignRandomTags();
        }

        // Esperamos un poco y luego cada jugador verifica si es el killer
        StartCoroutine(CheckAndTeleportKiller());
    }

    private IEnumerator CheckAndTeleportKiller()
    {
        // Esperamos a que se sincronicen las propiedades
        yield return new WaitForSeconds(1f);

        // Buscamos al jugador local
        PlayerModel localPlayer = null;
        PlayerModel[] allPlayers = FindObjectsOfType<PlayerModel>();

        foreach (PlayerModel player in allPlayers)
        {
            if (player.PhotonView.IsMine)
            {
                localPlayer = player;
                break;
            }
        }

        if (localPlayer == null) yield break;

        // Verificamos si el jugador local es el killer
        if (localPlayer.PhotonView.Owner.CustomProperties.TryGetValue("playerTag", out object tagValue))
        {
            string playerTag = tagValue.ToString();
            if (playerTag.ToLower() == "killer" && killerSpawnPoint != null)
            {
                // Si soy el killer, me teletransporto yo mismo
                localPlayer.transform.position = killerSpawnPoint.position;

                // Resetear velocidad si tiene Rigidbody
                Rigidbody rb = localPlayer.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                }

                Debug.Log($"Killer {localPlayer.PhotonView.Owner.NickName} teleported to spawn point");
            }
        }
    }
}