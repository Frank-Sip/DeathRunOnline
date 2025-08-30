using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerModel playerModel;
    private PlayerView playerView;
    private PlayerNickname playerUI;
    private bool cursorLocked = true;


    private void Start()
    {
        playerModel = GetComponent<PlayerModel>();
        playerView = GetComponent<PlayerView>();
        playerUI = GetComponent<PlayerNickname>();

        bool isLocalPlayer = playerModel.PhotonView.IsMine;
        //string playerName = playerModel.PhotonView.Owner.NickName;

        playerView.InitializeCamera(isLocalPlayer);
      //  playerUI.Initialize(playerName);
        
        if (isLocalPlayer)
        {
            SetCursorLock(true);
        }
    }

    private void Update()
    {
        if (playerModel.PhotonView.IsMine)
        {
            HandleCursorToggle();
            
            if (cursorLocked)
            {
                HandleMovementInput();
                HandleJumpInput();
            }
            
            UpdateGameplayLogic();
        }
        
        UpdateVisuals();
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = playerView.GetCameraForward();
        Vector3 right = playerView.GetCameraRight();
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        playerModel.Move(moveDirection, playerModel.GetRigidbodyVelocity());
    }

    private void HandleJumpInput()
    {
        bool jumpPressed = Input.GetButtonDown("Jump");
        playerModel.UpdateJumpBuffer(jumpPressed);

        if (playerModel.CanJump())
        {
            playerModel.Jump();
            playerModel.ConsumeJump();
        }
    }
    
    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorLock(!cursorLocked);
        }

        if (!cursorLocked && Input.GetMouseButtonDown(0))
        {
            SetCursorLock(true);
        }
    }
    
    private void SetCursorLock(bool lockCursor)
    {
        cursorLocked = lockCursor;
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateGameplayLogic()
    {
        playerModel.CheckGrounded();
    }

    private void UpdateVisuals()
    {
        playerUI.UpdateNameLabelOrientation();
    }
}