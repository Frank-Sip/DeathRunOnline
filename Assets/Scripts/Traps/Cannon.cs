using UnityEngine;
using Photon.Pun;

public class Cannon : MonoBehaviourPun, ITrap
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float launchForce = 500f;

    [PunRPC]
    public void RPC_ActivateTrap()
    {
        FireProjectile();
    }

    private void FireProjectile()
    {
        GameObject projectile = PhotonNetwork.Instantiate(projectilePrefab.name, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddForce(firePoint.forward * launchForce);
    }
}