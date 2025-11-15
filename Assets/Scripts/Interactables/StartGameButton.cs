using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class StartGameButton : MonoBehaviourPun, IInteractable
{
    [SerializeField] private GameTagManager gameTagManager;
    [SerializeField] private Transform killerSpawnPoint;
    [SerializeField] private GameObject preLobbyWall;
    [SerializeField] private int minimumPlayersRequired = 2;

    private bool isActive = true;
    private static float matchStartTime;

    public static float GetElapsedTime()
    {
        return Time.time - matchStartTime;
    }

    public void Interact()
    {
        if (!isActive) return;
        if (!HasMinimumPlayers())
        {
            Debug.Log($"cant start game. you need at least {minimumPlayersRequired} players.actual players {PhotonNetwork.PlayerList.Length}");
            return;
        }
        photonView.RPC("RPC_SetButtonActive", RpcTarget.All, false);
        photonView.RPC("RPC_StartGame", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private bool HasMinimumPlayers()
    {
        return PhotonNetwork.PlayerList.Length >= minimumPlayersRequired;
    }

    [PunRPC]
    private void RPC_SetButtonActive(bool active)
    {
        isActive = active;
    }

    [PunRPC]
    private void RPC_StartGame(int requesterActorNumber)
    {
        string playerName = PhotonNetwork.LocalPlayer.NickName;
        PlayerNameHelper.SetPlayerName(playerName);
        Debug.Log($"[StartGameButton] Nombre guardado en LootLocker: {playerName}");

        if (PhotonNetwork.LocalPlayer.ActorNumber == requesterActorNumber)
        {
            StartCoroutine(StartGameSequence());
        }
    }

    private IEnumerator StartGameSequence()
    {
        SetRoomPrivate();
        photonView.RPC("RPC_RemovePreLobbyWall", RpcTarget.All);

        gameTagManager.AssignRandomTags();

        matchStartTime = Time.time;
        photonView.RPC("RPC_SyncMatchStartTime", RpcTarget.AllBuffered, matchStartTime);
        Debug.Log($"[StartGameButton] Timer iniciado en: {matchStartTime}");

        GameManager.Instance.StartMatch();

        yield return new WaitForSeconds(0.5f);
        TeleportKillerToSpawn();
    }

    [PunRPC]
    private void RPC_SyncMatchStartTime(float startTime)
    {
        matchStartTime = startTime;
        Debug.Log($"[StartGameButton] Timer sincronizado: {matchStartTime}");
    }

    [PunRPC]
    private void RPC_RemovePreLobbyWall()
    {
        if (preLobbyWall != null)
        {
            preLobbyWall.SetActive(false);
            Debug.Log("Pre-lobby wall removed - Game area is now accessible");
        }
    }

    private void SetRoomPrivate()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            Room currentRoom = PhotonNetwork.CurrentRoom;
            currentRoom.IsOpen = false;
            currentRoom.IsVisible = false;
        }
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