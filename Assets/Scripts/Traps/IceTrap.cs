using UnityEngine;
using Photon.Pun;

public class IceTrap : MonoBehaviourPun, ITrap
{
    [Header("Spawning Objects")]
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);

    [Header("Ice Surface")]
    [SerializeField] private GameObject iceSurfaceObject; 

    private void Start()        
    {
        if (iceSurfaceObject != null)
        {
            iceSurfaceObject.SetActive(false);
        }
    }

    [PunRPC]
    public void RPC_ActivateTrap()
    {
        SpawnObjects();
        ActivateIceSurface();
    }

    private void SpawnObjects()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPosition = GetRandomSpawnPosition();
            PhotonNetwork.Instantiate(objectToSpawn.name, randomPosition, Quaternion.identity);
        }
    }

    private void ActivateIceSurface()
    {
        if (iceSurfaceObject != null)
        {
            iceSurfaceObject.SetActive(true);
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
        if (spawnPoint == null) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(spawnPoint.position, spawnAreaSize);
    }
}
