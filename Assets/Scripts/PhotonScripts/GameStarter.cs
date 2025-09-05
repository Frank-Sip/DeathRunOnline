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

    private void Start()
    {
        //if (!photonView.IsMine) return;

        StartCoroutine(CheckRoomStatus());
    }

    private IEnumerator CheckRoomStatus()
    {
        yield return new WaitUntil(()=> PhotonNetwork.InRoom);

        yield return new WaitForEndOfFrame();
        SpawnPlayer();

       

        //if (PhotonNetwork.InRoom && !hasSpawned)
        //{
        //    SpawnPlayer();
        //}
        //else if (!PhotonNetwork.InRoom)
        //{
        //    Debug.LogWarning("Not in room when GameStarter started. This shouldn't happen.");
        //    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        //}
    }

    //REvisar si ya no se llama en la otra escena
    //public override void OnJoinedRoom()
    //{
    //    if (!hasSpawned)
    //    {
    //        Debug.Log("GameStarter: Joined Room - Spawning Player");
    //        SpawnPlayer();
    //    }
    //}

    private void SpawnPlayer()
    {

        Transform spawn = GetPlayerSpawnPosition();

        //Instantiate(playerPrefab.gameObject, spawn.position, spawn.rotation);
        PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, spawn.rotation, 0);
        
        print("Player instantaited!");

        print(PhotonNetwork.CurrentRoom.PlayerCount);

    }

    private Transform GetPlayerSpawnPosition()
    {
        if (playerSpawnPoints.Count == 0) return playerSpawnPoint;
        Transform spawn = playerSpawnPoints[currentSpawnIndex % playerSpawnPoints.Count];
        currentSpawnIndex++;
        return spawn;
    }
}