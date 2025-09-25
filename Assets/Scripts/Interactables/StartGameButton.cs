using System.Collections;
using UnityEngine;
using Photon.Pun;

public class StartGameButton : MonoBehaviourPun, IInteractable
{
    [SerializeField] private GameTagManager gameTagManager;
    [SerializeField] private Transform killerSpawnPoint;
    
    private bool isActive = true;

    public void Interact()
    {
        if (!isActive) return;
        photonView.RPC("RPC_SetButtonActive", RpcTarget.All, false);
        photonView.RPC("RPC_StartGame", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    private void RPC_SetButtonActive(bool active)
    {
        isActive = active;
    }

    [PunRPC]
    private void RPC_StartGame(int requesterActorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == requesterActorNumber)
        {
            StartCoroutine(StartGameSequence());
        }
    }

    private IEnumerator StartGameSequence()
    {
        gameTagManager.AssignRandomTags();
        GameManager.Instance.StartMatch();
        yield return new WaitForSeconds(0.5f);
        TeleportKillerToSpawn();
    }

    private void TeleportKillerToSpawn()
    {
        PlayerModel[] allPlayers = FindObjectsOfType<PlayerModel>();

        foreach (PlayerModel player in allPlayers)
        {
            if (player.PhotonView.Owner.CustomProperties.TryGetValue("playerTag", out object tagValue))
            {
                string playerTag = tagValue.ToString();

                if (playerTag.ToLower() == "killer")
                {
                    player.PhotonView.RPC("RPC_TeleportPlayer", RpcTarget.All, killerSpawnPoint.position);
                    break;
                }
            }
        }
    }
}