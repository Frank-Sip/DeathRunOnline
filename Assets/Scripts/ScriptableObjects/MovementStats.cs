using UnityEngine;

[CreateAssetMenu(fileName = "Movement Stats", menuName = "Player/Movement Stats")]
public class MovementStats : ScriptableObject
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    
    [Header("Ground Detection")]
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float stunDuration = 2f;
    
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float JumpForce => jumpForce;
    public float CoyoteTime => coyoteTime;
    public float JumpBufferTime => jumpBufferTime;
    public float GroundCheckRadius => groundCheckRadius;
    public float GroundCheckDistance => groundCheckDistance;
    public float PushForce => pushForce;
    public float StunDuration => stunDuration;
}