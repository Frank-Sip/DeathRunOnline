using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Lava : MonoBehaviourPun, ITrap
{
    [SerializeField] private float activationHeight = 2f; 
    [SerializeField] private float activationDuration = 3f;
    [SerializeField] private float movementSpeed = 5f;

    private Vector3 initialPosition;
    private Vector3 activatedPosition;

    private void Start()
    {
        initialPosition = transform.localPosition;
        activatedPosition = initialPosition + Vector3.up * activationHeight;
    }

    [PunRPC]
    public void RPC_ActivateTrap()
    {
        StartCoroutine(MoveLava());
    }

    private IEnumerator MoveLava()
    {
        yield return MoveLavaObject(activatedPosition);
        yield return new WaitForSeconds(activationDuration);
        yield return MoveLavaObject(initialPosition);
    }

    private IEnumerator MoveLavaObject(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, movementSpeed * Time.deltaTime);
            yield return null;
        }
    }
}