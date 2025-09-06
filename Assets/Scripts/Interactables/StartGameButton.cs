using System.Collections;
using UnityEngine;
using Photon.Pun;

public class StartGameButton : MonoBehaviourPun, IInteractable
{
    [SerializeField] private GameTagManager gameTagManager;
    [SerializeField] private Transform killerSpawnPoint;

    public void Interact()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Solo el host puede iniciar el juego");
            return;
        }

        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        Debug.Log("Iniciando juego...");
        gameTagManager.AssignRandomTags();
        yield return new WaitForSeconds(0.5f);
        TeleportKillerToSpawn();

        Debug.Log("Juego iniciado correctamente");
    }

    private void TeleportKillerToSpawn()
    {
        if (killerSpawnPoint == null)
        {
            Debug.LogWarning("No se ha asignado punto de spawn para el killer");
            return;
        }

        PlayerModel[] allPlayers = FindObjectsOfType<PlayerModel>();

        foreach (PlayerModel player in allPlayers)
        {
            if (player.PhotonView.Owner.CustomProperties.TryGetValue("playerTag", out object tagValue))
            {
                string playerTag = tagValue.ToString();

                if (playerTag.ToLower() == "killer")
                {
                    player.PhotonView.RPC("RPC_TeleportPlayer", RpcTarget.All, killerSpawnPoint.position);

                    Debug.Log($"Killer {player.PhotonView.Owner.NickName} teletransportado al punto de spawn");
                    break;
                }
            }
        }
    }
}