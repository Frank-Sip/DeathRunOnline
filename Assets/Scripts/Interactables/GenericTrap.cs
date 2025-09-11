using UnityEngine;

public class GenericTrap : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        damageable.Die();
        Destroy(gameObject);
    }
}