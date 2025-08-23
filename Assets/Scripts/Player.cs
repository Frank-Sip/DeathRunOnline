using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float mousePitchTopLimit;
    [SerializeField] private float mousePitchLowLimit;
    [SerializeField] private float horizontalRotationSmoothness;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private Camera camera;
    
    private PhotonView photonView;
    private float cameraPitch;
    private float horizontalRotation;
    private Rigidbody rb;

    public PhotonView PhotonView => photonView ?? GetComponent<PhotonView>();

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        
        nameLabel.text = photonView.Owner.NickName;
        camera.enabled = PhotonView.IsMine;
        if (PhotonView.IsMine) Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Move();
        MouseLook();
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

        horizontalRotation += mouseX;
        float currentRotation = Mathf.LerpAngle(transform.eulerAngles.y, horizontalRotation, Time.deltaTime * horizontalRotationSmoothness);
        transform.eulerAngles = new Vector3(0f, currentRotation, 0f);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, mousePitchTopLimit, mousePitchLowLimit);
        camera.transform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
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
}
