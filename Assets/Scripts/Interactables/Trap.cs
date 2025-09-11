using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Trap : MonoBehaviourPun, IInteractable
{
    [SerializeField] private GameObject trapObject;
    
    public void Interact()
    {
        photonView.RPC("RPC_DestroyTrap", RpcTarget.All);
    }
    
    [PunRPC]
    private void RPC_DestroyTrap()
    {
        Destroy(trapObject);
    }
}
