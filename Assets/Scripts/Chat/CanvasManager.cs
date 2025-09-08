using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class CanvasManager : MonoBehaviour
{
    [Header("Chat UI")]
    public GameObject chatPanel;
    public PhotonChat2 photonChat;

    [Header("Player Controller Reference")]
    public PlayerController localPlayerController; 

    private bool chatOpen = false;
    private bool waitingForClickOutside = false;

    void Start()
    {
        chatPanel.SetActive(false);
        LockCursor(true);
        StartCoroutine(FindLocalPlayer());
    }

    IEnumerator FindLocalPlayer()
    {
        yield return new WaitForSeconds(0.5f);

        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in allPlayers)
        {
            PlayerModel model = player.GetComponent<PlayerModel>();
            if (model != null && model.PhotonView.IsMine)
            {
                localPlayerController = player;
                Debug.Log("Local player controller found and assigned");
                break;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!chatOpen)
            {
                OpenChat();
            }
            else
            {
                photonChat.HandleEnterPress(ref chatOpen, chatPanel, LockCursor, localPlayerController);
            }
        }

        if (chatOpen && Input.GetMouseButtonDown(0))
        {
            if (!waitingForClickOutside)
            {
                waitingForClickOutside = true;
                StartCoroutine(CheckClickOutside());
            }
        }
    }

    IEnumerator CheckClickOutside()
    {
        yield return null; 

        if (!IsPointerOverChat())
        {
            CloseChat();
        }

        waitingForClickOutside = false;
    }

    private bool IsPointerOverChat()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.transform.IsChildOf(chatPanel.transform) ||
                result.gameObject == chatPanel)
            {
                return true;
            }
        }

        return false;
    }

    private void OpenChat()
    {
        chatOpen = true;
        chatPanel.SetActive(true);

        LockCursor(false);

        if (localPlayerController != null)
        {
            localPlayerController.SetChatMode(true);
        }

        photonChat.chatInput.gameObject.SetActive(true);
        photonChat.chatInput.Select();
        photonChat.chatInput.ActivateInputField();

        Debug.Log("Chat opened");
    }

    public void CloseChat()
    {
        chatOpen = false;
        chatPanel.SetActive(false);

        LockCursor(true);

        if (localPlayerController != null)
        {
            localPlayerController.SetChatMode(false);
        }

        photonChat.chatInput.text = "";

        Debug.Log("Chat closed");
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
