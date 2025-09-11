using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPun
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;

    [Header("Layer Assignment")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask protectedLayer;

    private void Start()
    {
        var boxCol = GetComponent<BoxCollider>();
        boxCol.isTrigger = true;

        transform.position = spawnPoint.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView photonView = other.GetComponent<PhotonView>();
        if (photonView == null || !photonView.IsMine) return;

        int normalLayerIndex = GetLayerFromMask(playerLayer);

        if (other.gameObject.layer == normalLayerIndex)
        {
            int protectedLayerIndex = GetLayerFromMask(protectedLayer);
            photonView.RPC("RPC_ChangeLayer", RpcTarget.All, protectedLayerIndex);
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