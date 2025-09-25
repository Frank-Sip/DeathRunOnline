using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPun
{
    [Header("Layer Assignment")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask protectedLayer;
    
    private BoxCollider boxCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView photonView = other.GetComponent<PhotonView>();
        if (photonView == null || !photonView.IsMine) return;

        int normalLayerIndex = GetLayerFromMask(playerLayer);

        if (other.gameObject.layer == normalLayerIndex)
        {
            int protectedLayerIndex = GetLayerFromMask(protectedLayer);
            photonView.RPC("RPC_ChangeLayer", RpcTarget.AllBuffered, protectedLayerIndex);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PhotonView photonView = other.GetComponent<PhotonView>();
        if (photonView == null || !photonView.IsMine) return;

        int protectedLayerIndex = GetLayerFromMask(protectedLayer);

        if (other.gameObject.layer == protectedLayerIndex)
        {
            int normalLayerIndex = GetLayerFromMask(playerLayer);
            photonView.RPC("RPC_ChangeLayer", RpcTarget.All, normalLayerIndex);
        }
    }
    
    public Vector3 GetRandomSpawnPoint()
    {
        Vector3 center = boxCollider.bounds.center;
        Vector3 size = boxCollider.bounds.size;

        float randomX = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
        float randomZ = Random.Range(center.z - size.z / 2, center.z + size.z / 2);
        float y = center.y;

        return new Vector3(randomX, y, randomZ);
    }

    private int GetLayerFromMask(LayerMask layerMask)
    {
        for (int i = 0; i < 32; i++)
        {
            if ((layerMask.value & (1 << i)) != 0)
                return i;
        }
        return 0;
    }
}