using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CanvasManager : MonoBehaviour
{
    [Header("Chat UI")]
    public GameObject chatPanel;               
    public PhotonChat2 photonChat;             

    private bool chatOpen = false;

    void Start()
    {
        chatPanel.SetActive(false);
        LockCursor(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            chatOpen = !chatOpen;
            chatPanel.SetActive(chatOpen);

            if (chatOpen)
            {
                LockCursor(false);
                photonChat.chatInput.ActivateInputField();
            }
            else
            {
                LockCursor(true);
            }
        }
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}

public class PhotonChat : MonoBehaviourPunCallbacks
{
    public TMPro.TMP_InputField chatInput;
    public TMPro.TextMeshProUGUI chatDisplay;
    public PhotonView view;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(chatInput.text))
        {
            SendMessageToChat(chatInput.text);
            chatInput.text = "";
            chatInput.ActivateInputField(); 
        }
    }

    public void SendMessageToChat(string message)
    {
        view.RPC("ReceiveChatMessage", RpcTarget.AllBuffered, PhotonNetwork.NickName, message);
    }

    [PunRPC]
    void ReceiveChatMessage(string sender, string message)
    {
        chatDisplay.text += $"\n<b>{sender}:</b> {message}";
    }

    [PunRPC]
    void ReceiveChatMessage(string message)
    {
        chatDisplay.text += $"\n<b>{message}</b>";
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        view.RPC("ReceiveChatMessage", other, "Ha ingresado un nuevo jugador");
    }
}