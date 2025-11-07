using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerModel : MonoBehaviour, IPunObservable, IInteractable, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private MovementStats movementStats;
    [SerializeField] private LayerMask collisionMask;

    [Header("Ground / Jump Settings")]
    [SerializeField] private Transform groundCheckOrigin;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private int maxGroundHits = 5;

    [Header("Interaction Settings")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactionLayer;

    [Header("See Player Tags")]
    public KeyCode seeTagKey = KeyCode.Tab;

    private const string PLAYER_TAG_KEY = "playerTag";

    private PhotonView photonView;
    private Rigidbody rb;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isGrounded;
    private RaycastHit[] groundHits;
    private Collider[] interactables = new Collider[5];
    private bool isStunned = false;
    private float stunTimer = 0f;
    private bool isGrabbing = false;
    private bool isBeingGrabbed = false;
    private PlayerModel grabbedPlayer = null;
    private PlayerModel grabber = null;
    private float grabTimer = 0f;
    private const float grabDuration = 3f;
    private Vector3 grabOffset = Vector3.zero;

    public bool isAlive = true;
    public PhotonView PhotonView => photonView ?? GetComponent<PhotonView>();
    public bool IsGrounded => isGrounded;
    public float CoyoteTimeCounter => coyoteTimeCounter;
    public float JumpBufferCounter => jumpBufferCounter;
    public Action<PlayerModel> OnPlayerDeath;

    private float MoveSpeed => movementStats.MoveSpeed;
    private float RotationSpeed => movementStats.RotationSpeed;
    private float JumpForce => movementStats.JumpForce;
    private float CoyoteTime => movementStats.CoyoteTime;
    private float JumpBufferTime => movementStats.JumpBufferTime;
    private float GroundCheckRadius => movementStats.GroundCheckRadius;
    private float GroundCheckDistance => movementStats.GroundCheckDistance;
    private float PushForce => movementStats.PushForce;
    private float StunDuration => movementStats.StunDuration;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        groundHits = new RaycastHit[maxGroundHits];


        if (PhotonNetwork.InRoom)
        {
            UpdatePlayerTagFromProperties();
        }

        Camera myCam = GetComponentInChildren<Camera>();
        if (myCam != null && !PhotonView.IsMine)
        {
            myCam.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnPlayerPropertiesUpdate;
        }
    }

    private void OnDisable()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnPlayerPropertiesUpdate;
        }
    }

    private void Update()
    {
        if (!isAlive && PhotonView.IsMine)
        {
            HandleDeathCameraControls();
            return;
        }

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
            }
        }

        if (PhotonView.IsMine)
        {
            if (isGrabbing)
            {
                grabTimer -= Time.deltaTime;
                if (grabTimer <= 0)
                {
                    ReleaseGrab();
                }
            }

            if (isBeingGrabbed)
            {
                Vector3 targetPosition = grabber.transform.position + grabOffset;
                rb.position = Vector3.Lerp(rb.position, targetPosition, 10f * Time.deltaTime);
            }
        }
    }

    private void HandleDeathCameraControls()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            DeathCameraManager.Instance.ActivatePreviousCamera();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            DeathCameraManager.Instance.ActivateNextCamera();
        }
    }

    public void Interact()
    {
        PhotonView.RPC("RPC_PushPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private void OnPlayerPropertiesUpdate(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == 253)
        {
            UpdatePlayerTagFromProperties();
        }
    }

    public void UpdatePlayerTagFromProperties()
    {
        if (PhotonView.Owner.CustomProperties.TryGetValue(PLAYER_TAG_KEY, out object tagValue))
        {
            string playerTag = tagValue.ToString();
            PlayerNickname playerNickname = GetComponent<PlayerNickname>();
            playerNickname.SetPlayerTag(playerTag);
        }
    }

    public void Move(Vector3 moveDirection, Vector3 currentVelocity)
    {
        if (isStunned || !isAlive) return;
        rb.velocity = moveDirection * MoveSpeed + new Vector3(0, currentVelocity.y, 0);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
 
        }
    }

    public void Jump()
    {
        if (isStunned || !isAlive) return;
        rb.velocity = new Vector3(rb.velocity.x, JumpForce, rb.velocity.z);

    }

    public void CheckGrounded()
    {
        Vector3 sphereCenter = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position + Vector3.up * 0.1f;
        int hitCount = Physics.SphereCastNonAlloc(sphereCenter, GroundCheckRadius, Vector3.down, groundHits, GroundCheckDistance, groundMask);

        isGrounded = hitCount > 0;

        if (isGrounded)
        {
            coyoteTimeCounter = CoyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    public void TryInteract()
    {
        if (isStunned || !isAlive || isGrabbing || isBeingGrabbed) return;
        int elements = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRadius, interactables, interactionLayer);

        for (int i = 0; i < elements; i++)
        {
            var interactable = interactables[i];
            var interactableComponent = interactable.GetComponent<IInteractable>();

            if (interactableComponent != null)
            {
                interactableComponent.Interact();
                return;
            }
        }
    }

    public void TryGrab()
    {
        if (isStunned || !isAlive || isGrabbing || isBeingGrabbed) return;

        int elements = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRadius, interactables);

        for (int i = 0; i < elements; i++)
        {
            PlayerModel targetPlayer = interactables[i].GetComponent<PlayerModel>();

            if (targetPlayer != null && targetPlayer != this && targetPlayer.isAlive && !targetPlayer.isBeingGrabbed)
            {
                PhotonView.RPC("RPC_GrabPlayer", RpcTarget.All, targetPlayer.PhotonView.ViewID);
                return;
            }
        }
    }

    public void UpdateJumpBuffer(bool jumpPressed)
    {
        if (jumpPressed)
        {
            jumpBufferCounter = JumpBufferTime;
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

    public void ChangePlayerTag(string newTag)
    {
        PlayerNickname playerNickname = GetComponent<PlayerNickname>();
        if (playerNickname != null)
        {
            playerNickname.SetPlayerTag(newTag);
        }
    }

    public Vector3 GetRigidbodyVelocity()
    {
        return rb.velocity;
    }

    public void Die()
    {
        isAlive = false;

        bool wasRunner = IsPlayerRunner(PhotonView.Owner);

        GameTagManager.Instance.SetPlayerTag(PhotonView.Owner, "Dead");
        PhotonView.RPC("RPC_UpdateAliveState", RpcTarget.Others, false);
        OnPlayerDeath?.Invoke(this);

        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null) playerCollider.enabled = false;
        rb.isKinematic = true;

        Camera myCam = GetComponentInChildren<Camera>();
        if (myCam != null)
        {
            myCam.enabled = false;
        }

        if (PhotonView.IsMine)
        {
            DeathCameraManager.Instance.ActivateAnyCamera();

            if (wasRunner)
            {
                GameManager.Instance.photonView.RPC("RPC_DecrementRunnerCount", RpcTarget.All);
            }
        }
    }

    private bool IsPlayerRunner(Photon.Realtime.Player player)
    {
        if (player.CustomProperties.TryGetValue("playerTag", out object tagValue))
        {
            string playerTag = tagValue.ToString();
            return playerTag.ToLower() == "runner";
        }
        return false;
    }

    [PunRPC]
    private void RPC_UpdateAliveState(bool aliveState)
    {
        isAlive = aliveState;

        if (!aliveState)
        {
            Collider playerCollider = GetComponent<Collider>();
            if (playerCollider != null) playerCollider.enabled = false;
            rb.isKinematic = true;
        }
    }

    [PunRPC]
    public void RPC_PushPlayer(int pusherActorNumber)
    {
        if (!PhotonView.IsMine) return;

        var pusherPlayer = PhotonNetwork.CurrentRoom.GetPlayer(pusherActorNumber);
        PlayerModel[] allPlayers = FindObjectsOfType<PlayerModel>();
        PlayerModel pusher = null;

        foreach (PlayerModel player in allPlayers)
        {
            if (player.PhotonView.Owner.ActorNumber == pusherActorNumber)
            {
                pusher = player;
                break;
            }
        }
        
        Vector3 pushDirection = pusher.transform.forward;
        pushDirection.y = 0;

        rb.AddForce(pushDirection * PushForce, ForceMode.Impulse);

        isStunned = true;
        stunTimer = StunDuration;
        PlayerController controller = GetComponent<PlayerController>();
        controller.OnReceivePunch();
    }

    [PunRPC]
    public void RPC_GrabPlayer(int targetViewID)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView == null) return;

        PlayerModel targetPlayer = targetView.GetComponent<PlayerModel>();
        if (targetPlayer == null) return;

        if (PhotonView.IsMine)
        {
            isGrabbing = true;
            grabbedPlayer = targetPlayer;
            grabTimer = grabDuration;
            grabOffset = targetPlayer.transform.position - transform.position;
        }

        if (targetPlayer.PhotonView.IsMine)
        {
            targetPlayer.isBeingGrabbed = true;
            targetPlayer.grabber = this;
        }
    }

    private void ReleaseGrab()
    {
        PhotonView.RPC("RPC_ReleaseGrab", RpcTarget.All, grabbedPlayer.PhotonView.ViewID);
    }

    [PunRPC]
    public void RPC_ReleaseGrab(int targetViewID)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView == null) return;

        PlayerModel targetPlayer = targetView.GetComponent<PlayerModel>();

        if (PhotonView.IsMine)
        {
            isGrabbing = false;
            grabbedPlayer = null;
        }

        if (targetPlayer.PhotonView.IsMine)
        {
            targetPlayer.isBeingGrabbed = false;
            targetPlayer.grabber = null;
        }
    }

    [ContextMenu("GetID")]
    public void PrintID()
    {
        print(PhotonView.ViewID);
        print(PhotonNetwork.NickName);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.velocity);
        }
        else
        {
            Vector3 position = (Vector3)stream.ReceiveNext();
            Quaternion rotation = (Quaternion)stream.ReceiveNext();
            rb.velocity = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            
            position += rb.velocity * lag;
            rb.position = position;
            rb.rotation = rotation;
        }
    }

    [PunRPC]
    public void RPC_TeleportPlayer(Vector3 newPosition)
    {
        transform.position = newPosition;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
    }

    [PunRPC]
    public void RPC_ChangeLayer(int newLayer)
    {
        gameObject.layer = newLayer;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 sphereCenter = groundCheckOrigin.position;
        Gizmos.DrawWireSphere(sphereCenter, GroundCheckRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
    }
}
