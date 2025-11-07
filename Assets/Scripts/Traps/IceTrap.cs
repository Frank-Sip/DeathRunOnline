using UnityEngine;
using Photon.Pun;

public class IceTrap : MonoBehaviourPun, ITrap
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);

    [PunRPC]
    public void RPC_ActivateTrap()
    {
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPosition = GetRandomSpawnPosition();
            PhotonNetwork.Instantiate(objectToSpawn.name, randomPosition, Quaternion.identity);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float randomZ = Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f);
        
        Vector3 spawnPosition = spawnPoint.position + new Vector3(randomX, 0f, randomZ);
        return spawnPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(spawnPoint.position, spawnAreaSize);
    }
}
