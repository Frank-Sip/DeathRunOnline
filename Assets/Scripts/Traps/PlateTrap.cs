using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlateTrap : MonoBehaviourPun, ITrap
{
    [SerializeField] private float fallHeight = 10f;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float fallDelay = 0.5f;
    
    private bool canFall = false;
    private Vector3 initialPosition;
    private Vector3 fallenPosition;

    private void Start()
    {
        initialPosition = transform.localPosition;
        fallenPosition = initialPosition - Vector3.up * fallHeight;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canFall) return;
        PlayerModel player = collision.gameObject.GetComponent<PlayerModel>();
        StartCoroutine(FallCoroutine());
    }

    [PunRPC]
    public void RPC_ActivateTrap()
    {
        canFall = true;
    }

    private IEnumerator FallCoroutine()
    {
        yield return new WaitForSeconds(fallDelay);
        
        while (Vector3.Distance(transform.localPosition, fallenPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, fallenPosition, fallSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.localPosition = fallenPosition;
    }
}
