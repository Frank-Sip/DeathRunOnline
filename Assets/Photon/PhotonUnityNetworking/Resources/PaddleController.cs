using UnityEngine;
using Photon.Pun;

public class PaddleController : MonoBehaviourPun
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [Header("Input Settings")]
    [SerializeField] private KeyCode upKey = KeyCode.W;
    [SerializeField] private KeyCode downKey = KeyCode.S;
    [SerializeField] private KeyCode readyKey = KeyCode.Space;

    private int teamNumber;
    private bool isLocalPlayer;
    public bool isReady = false;

    private void Start()
    {
        isLocalPlayer = photonView.IsMine;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        HandleReadyInput();
        HandleMovement();
    }

    private void HandleReadyInput()
    {
        if (!isReady && Input.GetKeyDown(readyKey))
        {
            photonView.RPC("RPC_SetReady", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_SetReady()
    {
        isReady = true;

        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.OnPlayerReadyChanged();
        }

        Debug.Log($"Player {photonView.Owner.ActorNumber} is ready!");
    }

    private void HandleMovement()
    {
        float verticalInput = 0f;

        if (Input.GetKey(upKey))
        {
            verticalInput = 1f;
        }
        else if (Input.GetKey(downKey))
        {
            verticalInput = -1f;
        }

        if (verticalInput != 0f)
        {
            Vector3 movement = Vector3.up * verticalInput * moveSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + movement;
            float clampedY = Mathf.Clamp(newPosition.y, minY, maxY);
            float actualMovement = clampedY - transform.position.y;

            transform.Translate(0, actualMovement, 0, Space.World);
        }
    }

    public void SetTeam(int team)
    {
        teamNumber = team;
    }

    public int GetTeam()
    {
        return teamNumber;
    }
}
