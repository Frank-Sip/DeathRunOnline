using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Spikes : MonoBehaviourPun, ITrap
{
    [SerializeField] private GameObject spikeObject;
    [SerializeField] private float activationHeight = 2f; 
    [SerializeField] private float activationDuration = 3f;
    [SerializeField] private float movementSpeed = 5f;

    private Vector3 initialPosition;
    private Vector3 activatedPosition;

    private void Start()
    {
        initialPosition = spikeObject.transform.localPosition;
        activatedPosition = initialPosition + Vector3.up * activationHeight;

        spikeObject.SetActive(false);
    }

    [PunRPC]
    public void RPC_ActivateTrap()
    {
        StartCoroutine(ActivateSpikes());
    }

    private IEnumerator ActivateSpikes()
    {
        spikeObject.SetActive(true);
        yield return MoveSpikeObject(activatedPosition);

        yield return new WaitForSeconds(activationDuration);

        yield return MoveSpikeObject(initialPosition);
        spikeObject.SetActive(false);
    }

    private IEnumerator MoveSpikeObject(Vector3 targetPosition)
    {
        while (Vector3.Distance(spikeObject.transform.localPosition, targetPosition) > 0.01f)
        {
            spikeObject.transform.localPosition = Vector3.MoveTowards(spikeObject.transform.localPosition, targetPosition, movementSpeed * Time.deltaTime);
            yield return null;
        }
    }
}