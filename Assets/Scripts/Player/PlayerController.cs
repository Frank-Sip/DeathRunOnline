using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerModel playerModel;
    private PlayerView playerView;
    private PlayerNickname playerUI;
    private bool cursorLocked = true;
    private bool inChatMode = false; 

    private void Start()
    {
        playerModel = GetComponent<PlayerModel>();
        playerView = GetComponent<PlayerView>();
        playerUI = GetComponent<PlayerNickname>();

        bool isLocalPlayer = playerModel.PhotonView.IsMine;
        string playerName = playerModel.PhotonView.Owner.NickName;

        playerUI.Initialize(playerName);

        if (isLocalPlayer)
        {
            SetCursorLock(true);
        }
    }

    private void Update()
    {
        if (playerModel.PhotonView.IsMine)
        {
            if (!inChatMode)
            {
                HandleCursorToggle();
                HandleTabVisibility();

                if (cursorLocked)
                {
                    HandleMovementInput();
                    HandleJumpInput();
                    TryInteract();
                }
            }
            else
            {
                HandleTabVisibility();
            }

            UpdateGameplayLogic();
        }

        UpdateVisuals();
    }

    public void SetChatMode(bool enabled)
    {
        inChatMode = enabled;

        if (enabled)
        {
            Debug.Log("Player input disabled - Chat mode active");
        }
        else
        {
            Debug.Log("Player input enabled - Chat mode inactive");
        }
    }

    private void HandleTabVisibility()
    {
        bool tabPressed = Input.GetKey(playerModel.seeTagKey);
        PlayerModel[] allPlayers = FindObjectsOfType<PlayerModel>();

        foreach (PlayerModel player in allPlayers)
        {
            PlayerNickname playerNickname = player.GetComponent<PlayerNickname>();
            playerNickname.SetVisibility(tabPressed);
        }
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

    private void TryInteract()
    {
        if (Input.GetMouseButtonDown(0))
        {
            playerModel.TryInteract();
        }
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