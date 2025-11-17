using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerModel playerModel;
    private PlayerView playerView;
    private PlayerNickname playerUI;
    private bool cursorLocked = true;
    private bool inChatMode = false;

    [SerializeField] private Animator animator;
    private bool animatorReady = false;
    private bool wasGrounded = true;
    private bool wasMoving = false;

    public Animator modelSkinAnimator => playerView?.SkinModel;

    private void Start()
    {
        playerModel = GetComponent<PlayerModel>();
        playerView = GetComponent<PlayerView>();
        playerUI = GetComponent<PlayerNickname>();
        StartCoroutine(InitializeAnimator());

        bool isLocalPlayer = playerModel.PhotonView.IsMine;
        string playerName = playerModel.PhotonView.Owner.NickName;
        playerUI.Initialize(playerName);

        if (isLocalPlayer)
        {
            SetCursorLock(true);
        }
    }

    public void OnModelChanged()
    {
        animatorReady = false;
        animator = null;
        StartCoroutine(InitializeAnimator());
        Debug.Log("Model changed, reinitializing animator...");
    }

    private IEnumerator InitializeAnimator()
    {
        yield return new WaitForSeconds(0.3f);

        int maxAttempts = 15;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            if (playerView != null && playerView.SkinModel != null)
            {
                animator = playerView.SkinModel;

                if (animator.runtimeAnimatorController != null)
                {
                    animatorReady = true;
                    InitializeAnimatorStates();
                    break;
                }
            }

            attempts++;
            yield return new WaitForSeconds(0.1f);
        }

        if (!animatorReady)
        {
            Debug.LogError($"Failed to initialize animator after {maxAttempts} attempts");
        }
    }

    private void InitializeAnimatorStates()
    {
        if (!animatorReady) return;

        SetAnimatorBool("IsGrounded", playerModel.IsGrounded);
        SetAnimatorBool("IsRunning", false);
        SetAnimatorBool("IsFalling", false);

        // Inicializar los nuevos bools en false
        SetAnimatorBool("IsReceivingPunch", false);
        SetAnimatorBool("IsStunned", false);

        wasGrounded = playerModel.IsGrounded;
        wasMoving = false;
    }

    private void Update()
    {
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
            UpdateAnimatorStates();
        }

        UpdateVisuals();
    }

    private void UpdateAnimatorStates()
    {
        if (!animatorReady) return;

        bool isGrounded = playerModel.IsGrounded;
        Vector3 velocity = playerModel.GetRigidbodyVelocity();
        bool isMovingHorizontally = new Vector3(velocity.x, 0, velocity.z).magnitude > 0.1f;
        bool isFalling = !isGrounded && velocity.y < -0.1f;

        if (wasGrounded != isGrounded)
        {
            SetAnimatorBool("IsGrounded", isGrounded);
            wasGrounded = isGrounded;
        }

        if (isGrounded)
        {
            if (wasMoving != isMovingHorizontally)
            {
                SetAnimatorBool("IsRunning", isMovingHorizontally);
                wasMoving = isMovingHorizontally;
            }
        }
        else
        {
            if (wasMoving)
            {
                SetAnimatorBool("IsRunning", false);
                wasMoving = false;
            }
        }

        SetAnimatorBool("IsFalling", isFalling);
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

            if (animatorReady)
            {
                SetAnimatorTrigger("PunchTrigger");
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            playerModel.TryGrab();
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

            if (animatorReady && animator != null)
            {
                SetAnimatorTrigger("JumpTrigger");
            }
        }
    }

    public void OnReceivePunch()
    {
        if (animatorReady && animator != null)
        {
            SetAnimatorBool("IsReceivingPunch", true);

            StartCoroutine(ResetReceivePunchAfterDelay(0.5f)); 
        }
    }

    public void OnStunned()
    {
        if (animatorReady && animator != null)
        {
            SetAnimatorBool("IsStunned", true);

            StartCoroutine(ResetStunnedAfterDelay(playerModel.StunDuration));
        }
    }

    private IEnumerator ResetReceivePunchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetAnimatorBool("IsReceivingPunch", false);
    }

    private IEnumerator ResetStunnedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetAnimatorBool("IsStunned", false);
    }

    public void ResetStunAnimation()
    {
        if (animatorReady && animator != null)
        {
            SetAnimatorBool("IsStunned", false);
        }
    }

    private void SetAnimatorTrigger(string triggerName)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if (HasParameter(triggerName))
            {
                animator.SetTrigger(triggerName);
                Debug.Log($"Trigger activated: {triggerName}");
            }
            else
            {
                Debug.LogWarning($"Animator parameter '{triggerName}' not found in controller '{animator.runtimeAnimatorController.name}'");
            }
        }
    }

    private void SetAnimatorBool(string paramName, bool value)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if (HasParameter(paramName))
            {
                animator.SetBool(paramName, value);
            }
            else
            {
                Debug.LogWarning($"Animator parameter '{paramName}' not found in controller '{animator.runtimeAnimatorController.name}'");
            }
        }
    }

    private bool HasParameter(string paramName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }

        return false;
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

    public void ReinitializeAnimator()
    {
        StartCoroutine(InitializeAnimator());
    }
}