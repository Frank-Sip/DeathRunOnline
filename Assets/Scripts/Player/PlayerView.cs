using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("Visual Elements")]
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private Camera playerCamera;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float mousePitchTopLimit = -80f;
    [SerializeField] private float mousePitchLowLimit = 80f;
    [SerializeField] private float horizontalRotationSmoothness = 1f;

    private float cameraPitch;
    private float horizontalRotation;

    public float MouseSensitivity => mouseSensitivity;
    public Camera PlayerCamera => playerCamera;

    public void InitializeForLocalPlayer(string playerName)
    {
        nameLabel.text = playerName;
        playerCamera.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void InitializeForRemotePlayer(string playerName)
    {
        nameLabel.text = playerName;
        playerCamera.enabled = false;
    }

    public void UpdateMouseLook(float mouseX, float mouseY)
    {
        // Rotación horizontal del jugador
        transform.Rotate(Vector3.up * mouseX);

        // Rotación vertical de la cámara
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, mousePitchTopLimit, mousePitchLowLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    public void UpdateNameLabel(bool isLocalPlayer)
    {
        if (isLocalPlayer)
        {
            nameLabel.transform.rotation = Quaternion.LookRotation(nameLabel.transform.position - playerCamera.transform.position);
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

    public Vector3 GetCameraForward()
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }
}