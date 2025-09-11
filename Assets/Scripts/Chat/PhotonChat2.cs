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
    [SerializeField] public int maxMessages = 10;
    public ScrollRect scrollRect;

    private Queue<string> messages = new Queue<string>();

    void Start()
    {
        chatInput.onSubmit.AddListener(OnSubmit);

        SetupScrollView();
    }

    private void SetupScrollView()
    {
        if (scrollRect == null) return;

        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        
        ContentSizeFitter fitter = scrollRect.content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = scrollRect.content.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    void OnDestroy()
    {
        chatInput.onSubmit.RemoveListener(OnSubmit);
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
        AddMessage($"<color=#00ff00><b>{sender}:</b></color> {message}");
    }

    [PunRPC]
    void ReceiveChatMessage(string message)
    {
        AddMessage($"<color=#ffff00><b>{message}</b></color>");
    }

    private void AddMessage(string formattedMessage)
    {
        bool wasAtBottom = IsScrolledToBottom();

        messages.Enqueue(formattedMessage);

        while (messages.Count > maxMessages)
        {
            messages.Dequeue();
        }

        chatDisplay.text = string.Join("\n", messages);

        StartCoroutine(UpdateScrollAfterLayout(wasAtBottom));
    }

    IEnumerator UpdateScrollAfterLayout(bool wasAtBottom)
    {
        yield return null;

        if (scrollRect != null && scrollRect.content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            Canvas.ForceUpdateCanvases();

            if (wasAtBottom)
            {
                yield return null;
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }

    private bool IsScrolledToBottom()
    {
        if (scrollRect == null) return true;
        return scrollRect.verticalNormalizedPosition <= 0.05f;
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;
        yield return null;
        
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        if (photonView == null || photonView.ViewID == 0 || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Chat PhotonView not ready. Skipping welcome message.");
            return;
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        if (view == null || view.ViewID == 0 || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Chat PhotonView not ready. Skipping left room message.");
            return;
        }
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
            playerController.SetChatMode(false);
        }
    }
}