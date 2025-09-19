using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerModel playerModel;
    private PlayerView playerView;
    private PlayerNickname playerUI;
    private bool cursorLocked = true;
    private bool inChatMode = false;
    [SerializeField] Animator animator;

    public Animator modelSkinAnimator => playerView.SkinModel;

    private void Start()
    {
        playerModel = GetComponent<PlayerModel>();
        playerView = GetComponent<PlayerView>();
        playerUI = GetComponent<PlayerNickname>();

        StartCoroutine("GetSkinModel");
        

        bool isLocalPlayer = playerModel.PhotonView.IsMine;
        string playerName = playerModel.PhotonView.Owner.NickName;

        playerUI.Initialize(playerName);

        if (isLocalPlayer)
        {
            SetCursorLock(true);
        }
    }
    IEnumerator GetSkinModel()
    {
       yield return new WaitForSeconds(0.3f);
        animator = playerView.SkinModel;
    }

    private void Update()
    {
        if (animator == null) return;

        if (playerModel.PhotonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLock(!cursorLocked);
            }
            
            if (!inChatMode && playerModel.isAlive)
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
            else if (!playerModel.isAlive)
            {
                HandleTabVisibility();
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
        animator?.SetTrigger("IsRunning");
    }

    private void TryInteract()
    {
        if (Input.GetMouseButtonDown(0))
        {
            playerModel.TryInteract();
            animator.SetTrigger("PunchTrigger");
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
            animator.SetTrigger("JumpTrigger");
        }

    }

    private void HandleCursorToggle()
    {
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