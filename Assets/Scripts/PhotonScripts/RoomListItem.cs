using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button joinButton;

    private string roomName;

    public void SetupRoom(string name, int currentPlayers, int maxPlayers)
    {
        roomName = name;
        roomNameText.text = name;
        playerCountText.text = $"{currentPlayers}/{maxPlayers}";

        if (joinButton != null)
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(OnJoinButtonClicked);
            joinButton.interactable = (currentPlayers < maxPlayers);
        }
    }

    private void OnJoinButtonClicked()
    {
        PhotonManager.Instance.JoinRoomByName(roomName);
    }
}