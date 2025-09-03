using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomItemButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button roomButton;

    public string RoomName { get; set; }
    private int currentPlayers;
    private int maxPlayers;

    private void Awake()
    {
        if (roomButton == null)
            roomButton = GetComponent<Button>();

        if (roomButton != null)
            roomButton.onClick.AddListener(OnButtonPressed);
    }
    public void SetupRoom(string name, int currentPlayerCount, int maxPlayerCount)
    {
        RoomName = name;
        currentPlayers = currentPlayerCount;
        maxPlayers = maxPlayerCount;

        if (roomNameText != null)
            roomNameText.text = name;

        if (playerCountText != null)
            playerCountText.text = $"{currentPlayerCount}/{maxPlayerCount}";

        if (roomButton != null)
            roomButton.interactable = (currentPlayerCount < maxPlayerCount);
    }
    public void OnButtonPressed()
    {
        if (!string.IsNullOrEmpty(RoomName))
        {
            PhotonManager.Instance.JoinRoomByName(RoomName);
        }
    }
    public bool IsRoomFull()
    {
        return currentPlayers >= maxPlayers;
    }
}