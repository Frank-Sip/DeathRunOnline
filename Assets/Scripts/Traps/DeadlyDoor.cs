using UnityEngine;
using Photon.Pun;

public class DeadlyDoor : MonoBehaviourPun
{
    [SerializeField] private DeadlyDoorButtons[] requiredButtons;
    [SerializeField] private float openHeight = 5f;
    [SerializeField] private float openSpeed = 2f;

    private float targetHeight;
    private float startHeight;
    private bool shouldMove = false;

    private void Start()
    {
        startHeight = transform.position.y;
        targetHeight = startHeight + openHeight;
    }

    private void Update()
    {
        if (shouldMove && transform.position.y < targetHeight)
        {
            transform.Translate(Vector3.up * openSpeed * Time.deltaTime);
            
            if (transform.position.y >= targetHeight)
            {
                transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);
                shouldMove = false;
            }
        }
    }

    public void CheckButtons()
    {
        if (AreAllButtonsPressed())
        {
            OpenDoor();
        }
    }

    private bool AreAllButtonsPressed()
    {
        if (requiredButtons == null || requiredButtons.Length == 0)
        {
            return false;
        }

        foreach (var button in requiredButtons)
        {
            if (button == null || !button.IsPressed)
            {
                return false;
            }
        }

        return true;
    }

    private void OpenDoor()
    {
        shouldMove = true;
    }
}

