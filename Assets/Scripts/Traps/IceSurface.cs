using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IceSurface : MonoBehaviour
{
    [SerializeField] private float controlReduction = 0.5f; // Reducción de control sobre hielo (0 = sin control, 1 = control total)
    
    private void OnCollisionStay(Collision collision)
    {
        PlayerModel player = collision.gameObject.GetComponent<PlayerModel>();
        if (player != null && player.IsGrounded)
        {
            player.SetOnIce(true, controlReduction);
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        PlayerModel player = collision.gameObject.GetComponent<PlayerModel>();
        if (player != null)
        {
            player.SetOnIce(false, 1f);
        }
    }
}
