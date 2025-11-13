using System.Collections;
using UnityEngine;
using Photon.Pun;

public class FallingPlatform : MonoBehaviourPun, ITrap
{
    [SerializeField] protected float fallHeight = 2f;
    [SerializeField] protected float activationDuration = 3f;
    [SerializeField] protected float movementSpeed = 5f;

    protected Vector3 initialPosition;
    protected Vector3 fallenPosition;

    protected virtual void Start()
    {
        initialPosition = transform.localPosition;
        fallenPosition = initialPosition - Vector3.up * fallHeight;
    }

    [PunRPC]
    public void RPC_ActivateTrap()
    {
        StartCoroutine(MoveLava());
    }

    private IEnumerator MoveLava()
    {
        yield return MoveLavaObject(fallenPosition);
        yield return new WaitForSeconds(activationDuration);
        yield return MoveLavaObject(initialPosition);
    }

    protected IEnumerator MoveLavaObject(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, movementSpeed * Time.deltaTime);
            yield return null;
        }
    }
}