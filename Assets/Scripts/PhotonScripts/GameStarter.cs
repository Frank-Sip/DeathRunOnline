using UnityEngine;
using Photon.Pun;
using System.Collections;

public class GameStarter : MonoBehaviourPunCallbacks
{
    [SerializeField] private string playerPrefabName = "Player"; 
    [SerializeField] private SpawnManager spawnManager;

    private void Start()
    {
        StartCoroutine(SpawnWhenInRoom());
    }

    private IEnumerator SpawnWhenInRoom()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        SpawnAtManagerPoint();
    }

    private void SpawnAtManagerPoint()
    {
        Vector3 randomSpawnPosition = spawnManager.GetRandomSpawnPoint();

        PhotonNetwork.Instantiate(playerPrefabName, randomSpawnPosition, Quaternion.identity);

        Debug.Log($"Player spawned at {randomSpawnPosition} for {PhotonNetwork.LocalPlayer.NickName}");
    }
}