using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomItemButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button joinButton; // Cambio: ahora es el botón hijo específico

    public string RoomName { get; private set; }
    public int PlayerCount { get; private set; } 

    private int currentPlayers;
    private int maxPlayers;

    private void Awake()
    {
        if (joinButton == null)
        {
            joinButton = GetComponentInChildren<Button>();
        }

        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinButtonPressed);
        }
        else
        {
            Debug.LogError("No se encontró el botón Join en " + gameObject.name);
        }
    }

    public void SetupRoom(string name, int currentPlayerCount, int maxPlayerCount)
    {
        RoomName = name;
        currentPlayers = currentPlayerCount;
        maxPlayers = maxPlayerCount;
        PlayerCount = currentPlayerCount; 

        if (roomNameText != null)
            roomNameText.text = name;

        if (playerCountText != null)
            playerCountText.text = $"{currentPlayerCount}/{maxPlayerCount}";

        if (joinButton != null)
        {
            joinButton.interactable = (currentPlayerCount < maxPlayerCount);
        }
    }

    private void OnJoinButtonPressed()
    {
        if (!string.IsNullOrEmpty(RoomName) && !IsRoomFull())
        {
            Debug.Log($"Intentando unirse a la sala: {RoomName}");
            PhotonManager.Instance.JoinRoomByName(RoomName);
        }
    }

    public bool IsRoomFull()
    {
        return currentPlayers >= maxPlayers;
    }

   
}