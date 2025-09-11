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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        spectatorCamera = new GameObject("SpectatorCamera").AddComponent<Camera>();
        spectatorCamera.enabled = false;
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
        AttachToPlayer(alivePlayers[currentIndex]);
    }

    public void ActivateNextCamera()
    {
        if (alivePlayers.Count == 0) return;

        currentIndex = (currentIndex + 1) % alivePlayers.Count;
        AttachToPlayer(alivePlayers[currentIndex]);
    }

    public void ActivatePreviousCamera()
    {
        if (alivePlayers.Count == 0) return;

        currentIndex = (currentIndex - 1 + alivePlayers.Count) % alivePlayers.Count;
        AttachToPlayer(alivePlayers[currentIndex]);
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
            spectatorCamera.transform.position = target.transform.position + offset;
            spectatorCamera.transform.LookAt(target.transform.position + Vector3.up * 1.5f);
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
