using System;
using UnityEngine;
using Photon.Pun;

public class PushingTrap : MonoBehaviourPun
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private float stunDuration = 1.5f;

    private void OnCollisionEnter(Collision collision)
    {
        PlayerModel player = collision.gameObject.GetComponent<PlayerModel>();
        if (player == null) return;
        
        if (player.PhotonView.IsMine)
        {
            Vector3 pushDirection = transform.right;
            Vector3 finalPushDirection = pushDirection + Vector3.up * 0.5f;
            Vector3 force = finalPushDirection * pushForce;
            
            player.PhotonView.RPC("RPC_ApplyStun", RpcTarget.All, stunDuration);
            player.PhotonView.RPC("RPC_ApplyForce", RpcTarget.All, force);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.right * 2f);
    }
}
