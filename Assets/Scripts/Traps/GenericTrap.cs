using UnityEngine;
using Photon.Pun;

public class GenericTrap : MonoBehaviourPun
{
    [SerializeField] private bool destroyOnImpact = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
            if (targetView != null)
            {
                photonView.RPC("RPC_HandleCollision", RpcTarget.All, targetView.ViewID);
            }
        }

        if (destroyOnImpact)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    private void RPC_HandleCollision(int viewID)
    {
        PhotonView targetPhotonView = PhotonView.Find(viewID);
        if (targetPhotonView != null)
        {
            IDamageable damageable = targetPhotonView.GetComponent<IDamageable>();
            damageable?.Die();
        }
    }
}
