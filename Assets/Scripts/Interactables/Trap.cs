using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject trapObject;
    
    public void Interact()
    {
        Destroy(trapObject);
    }
}
