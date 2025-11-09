using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IceSurface : MonoBehaviour
{
    [SerializeField] private float iceSlipForce = 2f;
    [SerializeField] private float controlReduction = 0.7f; // 0 = sin control, 1 = control total
    
    private void OnCollisionStay(Collision collision)
    {
        PlayerModel player = collision.gameObject.GetComponent<PlayerModel>();
        if (player != null && player.IsGrounded)
        {
            Rigidbody rb = collision.rigidbody;
            if (rb != null)
            {
                player.SetOnIce(true, controlReduction);
                
                Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                if (horizontalVelocity.magnitude > 0.1f)
                {
                    rb.AddForce(horizontalVelocity.normalized * iceSlipForce, ForceMode.Force);
                }
            }
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        PlayerModel player = collision.gameObject.GetComponent<PlayerModel>();
        if (player != null)
        {
            // Notificar al jugador que salió del hielo
            player.SetOnIce(false, 1f);
        }
    }
}
