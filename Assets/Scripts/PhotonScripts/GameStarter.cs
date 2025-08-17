using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class GameStarter : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private List<Transform> playerSpawnPoints = new List<Transform>();
    private int currentSpawnIndex = 0;

    private void Start()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Transform spawn = GetPlayerSpawnPosition();
        PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, spawn.rotation, 0);
    }

    private Transform GetPlayerSpawnPosition()
    {
        if (playerSpawnPoints.Count == 0) return playerSpawnPoint;
        Transform spawn = playerSpawnPoints[currentSpawnIndex % playerSpawnPoints.Count];
        currentSpawnIndex++;
        return spawn;
    }
}