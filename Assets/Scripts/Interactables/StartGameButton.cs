using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameButton : MonoBehaviour, IInteractable
{
    [SerializeField] private GameTagManager gameTagManager;

    public void Interact()
    {
        gameTagManager.AssignRandomTags();
    }
}
