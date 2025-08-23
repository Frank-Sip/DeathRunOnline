using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private Camera camera;
    
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float mousePitchTopLimit;
    [SerializeField] private float mousePitchLowLimit;
    [SerializeField] private float horizontalRotationSmoothness;
    
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
    private float cameraPitch;
    private float horizontalRotation;
    private Rigidbody rb;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isGrounded;
    private RaycastHit[] groundHits;

    public PhotonView PhotonView => photonView ?? GetComponent<PhotonView>();

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        
        nameLabel.text = photonView.Owner.NickName;
        camera.enabled = PhotonView.IsMine;
        if (PhotonView.IsMine) Cursor.lockState = CursorLockMode.Locked;
        groundHits = new RaycastHit[maxGroundHits];
    }

    private void Update()
    {
        if (PhotonView.IsMine)
        {
            Move();
            MouseLook();
            CheckGrounded();
            HandleJumpInput();
        }
        SetNameLabel();
    }

    private void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        rb.velocity = moveDirection * moveSpeed + new Vector3(0, rb.velocity.y, 0);
    }

    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, mousePitchTopLimit, mousePitchLowLimit);
        camera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void SetNameLabel()
    {
        if (PhotonView.IsMine)
        {
            nameLabel.transform.rotation = Quaternion.LookRotation(nameLabel.transform.position - camera.transform.position);
        }
        else
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                nameLabel.transform.rotation = Quaternion.LookRotation(nameLabel.transform.position - mainCam.transform.position);
            }
        }
    }
    
    private void CheckGrounded()
    {
        Vector3 sphereCenter = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position + Vector3.up * 0.1f;

        int hitCount = Physics.SphereCastNonAlloc(sphereCenter, groundCheckRadius, Vector3.down, groundHits, groundCheckDistance, groundMask);

        bool wasGrounded = isGrounded;
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

    private void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
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
        Gizmos.color = Color.blue;
        Vector3 sphereCenter = groundCheckOrigin.position;
        Gizmos.DrawWireSphere(sphereCenter, groundCheckRadius);
    }
}
