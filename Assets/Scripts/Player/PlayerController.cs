using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerModel playerModel;
    private PlayerView playerView;

    private void Start()
    {
        // Obtener referencias a los componentes MVC
        playerModel = GetComponent<PlayerModel>();
        playerView = GetComponent<PlayerView>();

        // Inicializar la vista según si es jugador local o remoto
        if (playerModel.PhotonView.IsMine)
        {
            playerView.InitializeForLocalPlayer(playerModel.PhotonView.Owner.NickName);
        }
        else
        {
            playerView.InitializeForRemotePlayer(playerModel.PhotonView.Owner.NickName);
        }
    }

    private void Update()
    {
        if (playerModel.PhotonView.IsMine)
        {
            HandleMovementInput();
            HandleMouseInput();
            HandleJumpInput();
            UpdateGameplayLogic();
        }

        // Actualizar elementos visuales para todos los jugadores
        UpdateVisuals();
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Obtener direcciones de la cámara desde la vista
        Vector3 forward = playerView.GetCameraForward();
        Vector3 right = playerView.GetCameraRight();

        // Calcular dirección de movimiento
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;

        // Aplicar movimiento a través del modelo
        playerModel.Move(moveDirection, playerModel.GetRigidbodyVelocity());
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * playerView.MouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * playerView.MouseSensitivity;

        // Actualizar rotación de cámara a través de la vista
        playerView.UpdateMouseLook(mouseX, mouseY);
    }

    private void HandleJumpInput()
    {
        bool jumpPressed = Input.GetButtonDown("Jump");

        // Actualizar buffer de salto en el modelo
        playerModel.UpdateJumpBuffer(jumpPressed);

        // Verificar y ejecutar salto si es posible
        if (playerModel.CanJump())
        {
            playerModel.Jump();
            playerModel.ConsumeJump();
        }
    }

    private void UpdateGameplayLogic()
    {
        // Verificar si está en el suelo
        playerModel.CheckGrounded();
    }

    private void UpdateVisuals()
    {
        // Actualizar orientación del nombre del jugador
        playerView.UpdateNameLabel(playerModel.PhotonView.IsMine);
    }
}