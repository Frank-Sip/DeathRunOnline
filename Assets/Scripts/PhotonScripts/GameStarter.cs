using UnityEngine;
using Photon.Pun;
using System.Collections;

public class GameStarter : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView playerPrefab;
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
        Transform t = spawnManager.spawnPoint;
        PhotonNetwork.Instantiate(playerPrefab.name, t.position, t.rotation, 0);
    }
}