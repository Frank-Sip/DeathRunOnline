using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PhotonView))]
public class Projectile : MonoBehaviourPun
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (photonView.IsMine)
        {
            rb.isKinematic = false;
        }
        else
        {
            rb.isKinematic = true; // otros clientes solo ven sync
        }
    }
}
