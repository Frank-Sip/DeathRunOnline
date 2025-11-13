using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PunchingWall : MonoBehaviourPun, ITrap
{
    [Header("Punch Movement Settings")]
    [SerializeField] private float punchDistance = 3f;
    [SerializeField] private float punchSpeed = 10f;
    [SerializeField] private float holdDuration = 0.5f;

    [Header("Punch Objects")]
    [SerializeField] private Transform[] punchObjects;

    private Vector3[] initialPositions;
    private bool isActivated = false;

    private void Start()
    {
        if (punchObjects != null && punchObjects.Length > 0)
        {
            initialPositions = new Vector3[punchObjects.Length];
            for (int i = 0; i < punchObjects.Length; i++)
            {
                if (punchObjects[i] != null)
                {
                    initialPositions[i] = punchObjects[i].localPosition;
                }
            }
        }
    }

    [PunRPC]
    public void RPC_ActivateTrap()
    {
        if (!isActivated)
        {
            StartCoroutine(PunchSequence());
        }
    }

    private IEnumerator PunchSequence()
    {
        isActivated = true;

        yield return MovePunches(punchDistance);

        yield return new WaitForSeconds(holdDuration);

        yield return MovePunches(0f);

        isActivated = false;
    }

    private IEnumerator MovePunches(float targetDistance)
    {
        if (punchObjects == null || punchObjects.Length == 0)
        {
            yield break;
        }

        bool moving = true;

        while (moving)
        {
            moving = false;

            for (int i = 0; i < punchObjects.Length; i++)
            {
                if (punchObjects[i] == null) continue;

                Vector3 targetPosition = initialPositions[i] + Vector3.forward * targetDistance;
                Vector3 currentPosition = punchObjects[i].localPosition;

                if (Vector3.Distance(currentPosition, targetPosition) > 0.01f)
                {
                    punchObjects[i].localPosition = Vector3.MoveTowards(
                        currentPosition,
                        targetPosition,
                        punchSpeed * Time.deltaTime
                    );
                    moving = true;
                }
                else
                {
                    punchObjects[i].localPosition = targetPosition;
                }
            }

            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (punchObjects == null) return;

        Gizmos.color = Color.red;

        foreach (Transform punch in punchObjects)
        {
            if (punch != null)
            {
                Vector3 startPos = Application.isPlaying ? punch.position : punch.TransformPoint(Vector3.zero);
                Vector3 endPos = startPos + punch.forward * punchDistance;
                
                Gizmos.DrawLine(startPos, endPos);
                Gizmos.DrawWireSphere(endPos, 0.3f);
            }
        }
    }
}
