using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class GameStarter : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private List<Transform> playerSpawnPoints = new List<Transform>();
    private int currentSpawnIndex = 0;
    private bool hasSpawned = false;

    private void Start()
    {
        StartCoroutine(CheckRoomStatus());
    }

    private IEnumerator CheckRoomStatus()
    {
        yield return null; 

        if (PhotonNetwork.InRoom && !hasSpawned)
        {
            SpawnPlayer();
        }
        else if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Not in room when GameStarter started. This shouldn't happen.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public override void OnJoinedRoom()
    {
        if (!hasSpawned)
        {
            Debug.Log("GameStarter: Joined Room - Spawning Player");
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        if (hasSpawned) return;

        hasSpawned = true;
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