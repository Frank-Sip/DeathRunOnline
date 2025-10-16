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

    // NUEVO: Para colores 3D
    [Header("Visual Settings")]
    [SerializeField] private Renderer paddleRenderer; // Arrastra el MeshRenderer aquí en el inspector

    private int teamNumber;
    private bool isLocalPlayer;
    public bool isReady = false;
    private Material paddleMaterial;

    private void Start()
    {
        isLocalPlayer = photonView.IsMine;

        // NUEVO: Configurar material
        SetupPaddleMaterial();

        // Aplicar color del jugador
        if (ColorManager.Instance != null)
        {
            Color playerColor = ColorManager.Instance.GetPlayerColor(photonView.Owner.ActorNumber);
            ApplyColor(playerColor);
        }
    }

    private void SetupPaddleMaterial()
    {
        // Si no asignaste el renderer en el inspector, búscalo
        if (paddleRenderer == null)
        {
            paddleRenderer = GetComponent<Renderer>();
            if (paddleRenderer == null)
            {
                paddleRenderer = GetComponentInChildren<Renderer>();
            }
        }

        // Crear una instancia del material para este paddle
        if (paddleRenderer != null)
        {
            paddleMaterial = new Material(paddleRenderer.material);
            paddleRenderer.material = paddleMaterial;
        }
        else
        {
            Debug.LogError($"[PaddleController] No se encontró Renderer en {gameObject.name}");
        }
    }

    private void ApplyColor(Color color)
    {
        if (paddleMaterial != null)
        {
            // Para Standard Shader
            paddleMaterial.color = color;

            // Si usas URP/HDRP, también puedes usar:
            // paddleMaterial.SetColor("_BaseColor", color);
        }
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

    private void OnDestroy()
    {
        // Limpiar el material instanciado
        if (paddleMaterial != null)
        {
            Destroy(paddleMaterial);
        }
    }
}