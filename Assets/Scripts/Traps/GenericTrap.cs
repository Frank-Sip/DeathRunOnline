using UnityEngine;
using Photon.Pun;

public class GenericTrap : MonoBehaviourPun
{
    [SerializeField] private bool destroyOnImpact = true;
    
    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            photonView.RPC("RPC_HandleCollision", RpcTarget.All, collision.gameObject.GetComponent<PhotonView>().ViewID);
        }
        
        if (!destroyOnImpact) return;
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void RPC_HandleCollision(int viewID)
    {
        PhotonView targetPhotonView = PhotonView.Find(viewID);
        IDamageable damageable = targetPhotonView.GetComponent<IDamageable>();
        damageable.Die();
    }
}