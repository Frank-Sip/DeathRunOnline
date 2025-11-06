using UnityEngine;
using Photon.Pun;

public class DeadlyDoorButtons : MonoBehaviourPun, IInteractable
{
    [SerializeField] private Material pressedMaterial;
    [SerializeField] private Material unpressedMaterial;
    [SerializeField] private DeadlyDoor door;
    
    private bool isPressed = false;
    private Vector3 originalPosition;
    private MeshRenderer meshRenderer;
    
    public bool IsPressed => isPressed;

    private void Start()
    {
        originalPosition = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = unpressedMaterial;
    }

    public void Interact()
    {
        if (!isPressed)
        {
            photonView.RPC("RPC_PressButton", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_PressButton()
    {
        isPressed = true;
        meshRenderer.material = pressedMaterial;
        door.CheckButtons();
    }
}
