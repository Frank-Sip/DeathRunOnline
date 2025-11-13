using UnityEngine;
using Photon.Pun;

public class PushingTrap : MonoBehaviourPun, ITrap
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private float stunDuration = 1.5f;
    [SerializeField] private float upwardForceMultiplier = 0.5f; 

    private bool isActivated = false;

    [PunRPC]
    public void RPC_ActivateTrap()
    {
        isActivated = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActivated) return;

        PlayerModel player = collision.gameObject.GetComponent<PlayerModel>();

        if (player != null && player.PhotonView.IsMine)
        {
            Vector3 pushDirection = transform.right;

            pushDirection.y = upwardForceMultiplier;
            pushDirection.Normalize();

            Rigidbody playerRb = player.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);

                player.PhotonView.RPC("RPC_ApplyStun", RpcTarget.All, stunDuration);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3 pushDir = transform.right;
        pushDir.y = upwardForceMultiplier;
        pushDir.Normalize();

        Gizmos.DrawRay(transform.position, pushDir * 2f);
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>()?.bounds.size ?? Vector3.one);
    }
}
