using UnityEngine;

public class TrapTeleportManager : MonoBehaviour
{
    public static TrapTeleportManager Instance { get; private set; }

    [Header("Trap Teleportation Points")]
    [SerializeField] private Transform[] trapTeleportPoints;
    [SerializeField] private float trapHeightOffset = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ValidateTrapPoints();
    }

    private void ValidateTrapPoints()
    {

        int validPoints = 0;
        for (int i = 0; i < trapTeleportPoints.Length; i++)
        {
            if (trapTeleportPoints[i] != null)
            {
                validPoints++;
            }
        }

    }

    public int TrapCount => trapTeleportPoints != null ? trapTeleportPoints.Length : 0;

    public bool HasTraps => trapTeleportPoints != null && trapTeleportPoints.Length > 0;

    public Vector3 GetTrapPosition(int index)
    {
        if (trapTeleportPoints == null || trapTeleportPoints.Length == 0)
        {
            return Vector3.zero;
        }

        if (index < 0 || index >= trapTeleportPoints.Length)
        {
            return Vector3.zero;
        }

        if (trapTeleportPoints[index] == null)
        {
            return Vector3.zero;
        }

        Vector3 position = trapTeleportPoints[index].position;
        position.y += trapHeightOffset;
        return position;
    }

    public int GetNextIndex(int currentIndex)
    {
        if (!HasTraps) return 0;
        return (currentIndex + 1) % trapTeleportPoints.Length;
    }

    public int GetPreviousIndex(int currentIndex)
    {
        if (!HasTraps) return 0;
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = trapTeleportPoints.Length - 1;
        }
        return currentIndex;
    }

    private void OnDrawGizmos()
    {
        if (trapTeleportPoints == null || trapTeleportPoints.Length == 0)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < trapTeleportPoints.Length; i++)
        {
            if (trapTeleportPoints[i] != null)
            {
                Vector3 position = trapTeleportPoints[i].position;
                position.y += trapHeightOffset;

                Gizmos.DrawWireSphere(position, 0.5f);

                int nextIndex = (i + 1) % trapTeleportPoints.Length;
                if (trapTeleportPoints[nextIndex] != null)
                {
                    Vector3 nextPosition = trapTeleportPoints[nextIndex].position;
                    nextPosition.y += trapHeightOffset;
                    Gizmos.DrawLine(position, nextPosition);
                }
            }
        }
    }
}