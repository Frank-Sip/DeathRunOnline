using System.Collections;
using UnityEngine;
using Photon.Pun;

public class ButtonTrap : MonoBehaviourPun, IInteractable
{
    [SerializeField] private MonoBehaviour[] trapObjects;
    [SerializeField] private float cooldownDuration = 5f;
    
    private bool isOnCooldown = false;

    public void Interact()
    {
        if (isOnCooldown) return;
        
        foreach (var trap in trapObjects)
        {
            ITrap trapInterface = trap.GetComponent<ITrap>();
            if (trapInterface != null)
            {
                PhotonView trapPhotonView = trap.GetComponent<PhotonView>();
                trapPhotonView.RPC("RPC_ActivateTrap", RpcTarget.All);
            }
        }
        
        StartCoroutine(StartCooldown());
    }
    
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }
}