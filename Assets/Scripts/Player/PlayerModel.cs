using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask collisionMask;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private Transform groundCheckOrigin;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private int maxGroundHits = 5;

    private PhotonView photonView;
    private Rigidbody rb;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isGrounded;
    private RaycastHit[] groundHits;

    public PhotonView PhotonView => photonView ?? GetComponent<PhotonView>();
    public bool IsGrounded => isGrounded;
    public float CoyoteTimeCounter => coyoteTimeCounter;
    public float JumpBufferCounter => jumpBufferCounter;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        groundHits = new RaycastHit[maxGroundHits];
    }

    public void Move(Vector3 moveDirection, Vector3 currentVelocity)
    {
        rb.velocity = moveDirection * moveSpeed + new Vector3(0, currentVelocity.y, 0);
    }

    public void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    public void CheckGrounded()
    {
        Vector3 sphereCenter = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position + Vector3.up * 0.1f;
        int hitCount = Physics.SphereCastNonAlloc(sphereCenter, groundCheckRadius, Vector3.down, groundHits, groundCheckDistance, groundMask);

        isGrounded = hitCount > 0;

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    public void UpdateJumpBuffer(bool jumpPressed)
    {
        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    public bool CanJump()
    {
        return jumpBufferCounter > 0 && coyoteTimeCounter > 0;
    }

    public void ConsumeJump()
    {
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
    }

    public Vector3 GetRigidbodyVelocity()
    {
        return rb.velocity;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!PhotonView.IsMine) return;

        if ((collisionMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            PhotonView.RPC("RPC_InformCollision", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName);
        }
    }

    [PunRPC]
    public void RPC_InformCollision(string playerName)
    {
        Debug.Log($"{playerName} has collided with an object.");
    }

    [ContextMenu("GetID")]
    public void PrintID()
    {
        print(PhotonView.ViewID);
        print(PhotonNetwork.NickName);
    }

    private void OnDrawGizmos()
    {
        if (groundCheckOrigin != null)
        {
            Gizmos.color = Color.blue;
            Vector3 sphereCenter = groundCheckOrigin.position;
            Gizmos.DrawWireSphere(sphereCenter, groundCheckRadius);
        }
    }
}