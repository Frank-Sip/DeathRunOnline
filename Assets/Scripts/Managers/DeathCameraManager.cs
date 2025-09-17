using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DeathCameraManager : MonoBehaviour
{
    public static DeathCameraManager Instance { get; private set; }

    private List<PlayerModel> alivePlayers = new List<PlayerModel>();
    private int currentIndex = 0;

    private Camera spectatorCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -6);

    private float yaw = 0f;
    private float pitch = 15f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        spectatorCamera = new GameObject("SpectatorCamera").AddComponent<Camera>();
        spectatorCamera.enabled = false;
    }

    private void Update()
    {
        if (spectatorCamera.enabled)
        {
            HandleSpectatorCameraInput();
        }
    }

    private void HandleSpectatorCameraInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    public void RefreshAlivePlayers()
    {
        alivePlayers.Clear();
        foreach (PlayerModel player in FindObjectsOfType<PlayerModel>())
        {
            if (player.isAlive)
            {
                alivePlayers.Add(player);
            }
        }
    }

    public void ActivateAnyCamera()
    {
        RefreshAlivePlayers();
        if (alivePlayers.Count == 0) return;

        currentIndex = 0;
        ResetSpectatorRotation();
        AttachToPlayer(alivePlayers[currentIndex]);
    }

    public void ActivateNextCamera()
    {
        if (alivePlayers.Count == 0) return;

        currentIndex = (currentIndex + 1) % alivePlayers.Count;
        ResetSpectatorRotation();
        AttachToPlayer(alivePlayers[currentIndex]);
    }

    public void ActivatePreviousCamera()
    {
        if (alivePlayers.Count == 0) return;

        currentIndex = (currentIndex - 1 + alivePlayers.Count) % alivePlayers.Count;
        ResetSpectatorRotation();
        AttachToPlayer(alivePlayers[currentIndex]);
    }

    private void ResetSpectatorRotation()
    {
        yaw = 0f;
        pitch = 15f;
    }

    private void AttachToPlayer(PlayerModel player)
    {
        if (player == null) return;

        spectatorCamera.enabled = true;
        StopAllCoroutines();
        StartCoroutine(FollowPlayer(player));
    }

    private IEnumerator FollowPlayer(PlayerModel target)
    {
        while (target != null && target.isAlive)
        {
            Vector3 targetPos = target.transform.position + offset;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

            spectatorCamera.transform.position = targetPos;
            spectatorCamera.transform.rotation = rotation;

            yield return null;
        }

        RefreshAlivePlayers();
        if (alivePlayers.Count > 0)
        {
            ActivateAnyCamera();
        }
        else
        {
            spectatorCamera.enabled = false;
        }
    }
}