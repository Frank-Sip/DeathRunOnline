using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float mousePitchTopLimit = -80f;
    [SerializeField] private float mousePitchLowLimit = 80f;

    private float cameraPitch;

    public float MouseSensitivity => mouseSensitivity;
    public Camera PlayerCamera => playerCamera;

    public void InitializeCamera(bool isLocalPlayer)
    {
        playerCamera.enabled = isLocalPlayer;
    }

    public void UpdateMouseLook(float mouseX, float mouseY)
    {
        transform.Rotate(Vector3.up * mouseX);
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, mousePitchTopLimit, mousePitchLowLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
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