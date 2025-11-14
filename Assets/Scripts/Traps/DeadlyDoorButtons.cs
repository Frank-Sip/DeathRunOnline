using UnityEngine;
using Photon.Pun;

public class DeadlyDoorButtons : MonoBehaviourPun, IInteractable
{
    [SerializeField] private bool isPressed = false;
    [SerializeField] private Material pressedMaterial;
    [SerializeField] private Material unpressedMaterial;
    [SerializeField] private DeadlyDoor door;
    [SerializeField] private float pressDuration = 1f;
    
    private Vector3 originalPosition;
    private MeshRenderer meshRenderer;
    private float currentPressDuration;
    
    public bool IsPressed => isPressed;

    private void Start()
    {
        originalPosition = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = unpressedMaterial;
    }

    private void Update()
    {
        if (isPressed)
        {
            currentPressDuration -= Time.deltaTime;

            if (currentPressDuration <= 0f)
            {
                photonView.RPC("RPC_UnpressButton", RpcTarget.All);
                currentPressDuration = pressDuration;
            }
        }
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

    [PunRPC]
    private void RPC_UnpressButton()
    {
        isPressed = false;
        meshRenderer.material = unpressedMaterial;
        door.CheckButtons();
    }
}

