using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PhotonChat2 : MonoBehaviourPunCallbacks
{
    public TMPro.TMP_InputField chatInput;
    public TMPro.TextMeshProUGUI chatDisplay;
    public PhotonView view;

    [Header("Chat Settings")]
    [SerializeField] public int maxMessages = 50;
    public ScrollRect scrollRect;

    private Queue<string> messages = new Queue<string>();

    void Start()
    {
        if (chatInput != null)
        {
            chatInput.onSubmit.AddListener(OnSubmit);
        }
    }

    void OnDestroy()
    {
        if (chatInput != null)
        {
            chatInput.onSubmit.RemoveListener(OnSubmit);
        }
    }

    private void OnSubmit(string text) { }

    public void SendMessageToChat(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        view.RPC("ReceiveChatMessage", RpcTarget.AllBuffered, PhotonNetwork.NickName, message);
    }

    [PunRPC]
    void ReceiveChatMessage(string sender, string message)
    {
        message = message.Replace("<", "").Replace(">", "");
        AddMessage($"\n<color=#00ff00><b>{sender}:</b></color> {message}");
    }

    [PunRPC]
    void ReceiveChatMessage(string message)
    {
        AddMessage($"\n<color=#ffff00><b>{message}</b></color>");
    }

    private void AddMessage(string formattedMessage)
    {
        bool wasAtBottom = IsScrolledToBottom();

        messages.Enqueue(formattedMessage);
        if (messages.Count > maxMessages)
            messages.Dequeue();

        chatDisplay.text = string.Join("", messages);

        if (wasAtBottom)
            StartCoroutine(ScrollToBottom());
    }

    private bool IsScrolledToBottom()
    {
        if (scrollRect == null) return true;
        return scrollRect.verticalNormalizedPosition <= 0.001f;
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        view.RPC("ReceiveChatMessage", RpcTarget.All, $"{other.NickName} ha ingresado a la sala");
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        view.RPC("ReceiveChatMessage", RpcTarget.All, $"{other.NickName} ha salido de la sala");
    }

    public void HandleEnterPress(ref bool chatOpen, GameObject chatPanel, System.Action<bool> LockCursor, PlayerController playerController)
    {
        if (!string.IsNullOrWhiteSpace(chatInput.text))
        {
            SendMessageToChat(chatInput.text);
            chatInput.text = "";
            chatInput.ActivateInputField();
        }
        else
        {
            chatOpen = false;
            chatPanel.SetActive(false);
            LockCursor(true);

            if (playerController != null)
            {
                playerController.SetChatMode(false);
            }
        }
    }
}