using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class UIManager : MonoBehaviourPunCallbacks
{
    public static UIManager Instance;

    [Header("Canvas References")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject lobbyPanel;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField roomNameInput;

    [Header("Buttons")]
    [SerializeField] private Button connectionButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button joinRandomButton;
    [SerializeField] private Button refreshButton;

    [Header("Room List")]
    [SerializeField] private Transform roomListParent;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private TMP_Text roomCountText;
    [SerializeField] private TMP_Text playerCountText;

    [Header("Skin Configuration")]
    [SerializeField] private Button[] skinButtons;
    [SerializeField] private Image skinPreview;
    [SerializeField] private PlayerSkinConfig skinConfig;

    private const string nickNameKey = "playerNickname";
    private const string skinKey = "playerSkin";

    private string nickname;
    private int selectedSkinIndex = 0;
    private List<GameObject> roomListItems = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"UIManager Start in scene: {currentScene}");

        if (IsMenuScene())
        {
            InitializeMenuUI();
        }
        else
        {
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
            if (lobbyPanel != null) lobbyPanel.SetActive(false);
        }
    }

    private bool IsMenuScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return sceneName == "MainMenu" || sceneName == "Menu" || sceneName == "Lobby";
    }

    private void InitializeMenuUI()
    {
        InitializeUI();
        SetupButtons();
        SetupSkinButtons();
        SelectSkin(0);

        if (PlayerPrefs.HasKey(nickNameKey))
        {
            nameInput.text = PlayerPrefs.GetString(nickNameKey);
            nickname = nameInput.text;
        }

        VerifyName(nameInput.text);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("Room properties updated");
    }

    private void InitializeUI()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
    }

    private void SetupButtons()
    {
        if (connectionButton != null) connectionButton.onClick.AddListener(OnConnectButton);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueButton);
        if (nameInput != null) nameInput.onValueChanged.AddListener(VerifyName);

        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(OnCreateRoomButton);
        if (joinRoomButton != null)
            joinRoomButton.onClick.AddListener(OnJoinRoomByNameButton);
        if (joinRandomButton != null)
            joinRandomButton.onClick.AddListener(OnJoinRandomButton);
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshButton);
    }

    private void VerifyName(string name)
    {
        bool isValid = !string.IsNullOrWhiteSpace(name) && name.Length >= 3;
        if (connectionButton != null) connectionButton.interactable = isValid;
        if (continueButton != null) continueButton.interactable = isValid;

        if (isValid)
            nickname = name;
    }

    private void SetupSkinButtons()
    {
        if (skinConfig == null) return;

        int skinCount = skinConfig.GetSkinCount();

        for (int i = 0; i < skinButtons.Length && i < skinCount; i++)
        {
            int index = i;
            skinButtons[i].onClick.AddListener(() => SelectSkin(index));

            var skinData = skinConfig.GetSkinData(i);
            if (skinData.skinIcon != null)
            {
                Image buttonImage = skinButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                    buttonImage.sprite = skinData.skinIcon;
            }

            skinButtons[i].gameObject.SetActive(true);
        }

        for (int i = skinCount; i < skinButtons.Length; i++)
        {
            skinButtons[i].gameObject.SetActive(false);
        }
    }

    private void SelectSkin(int skinIndex)
    {
        if (skinConfig == null) return;

        selectedSkinIndex = skinIndex;
        var skinData = skinConfig.GetSkinData(skinIndex);

        if (skinData.skinIcon != null && skinPreview != null)
        {
            skinPreview.sprite = skinData.skinIcon;
        }

        for (int i = 0; i < skinButtons.Length; i++)
        {
            if (skinButtons[i] != null)
                skinButtons[i].interactable = (i != skinIndex);
        }
    }

    private void OnContinueButton()
    {
        SavePlayerPreferences();
        ConnectToPhoton();
    }

    private void OnConnectButton()
    {
        SavePlayerPreferences();
        ConnectToPhoton();
    }

    private void SavePlayerPreferences()
    {
        PlayerPrefs.SetString(nickNameKey, nickname);
        PlayerPrefs.SetInt(skinKey, selectedSkinIndex);
        PlayerPrefs.Save();
    }

    private void ConnectToPhoton()
    {
        if (connectionButton != null) connectionButton.interactable = false;
        if (continueButton != null) continueButton.interactable = false;
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);

        PhotonManager.Instance.ConnectToPhoton(nickname, selectedSkinIndex);
    }

    public void ShowLobbyPanel()
    {
        if (!IsMenuScene()) return;

        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        UpdateRoomListUI();
    }

    public void OnSceneChanged()
    {
        if (!IsMenuScene())
        {
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
            if (lobbyPanel != null) lobbyPanel.SetActive(false);
        }
    }
    public void OnCreateRoomButton()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
            roomName = $"{nickname}'s Room";

        PhotonManager.Instance.JoinOrCreateRoom(roomName);
    }

    private void OnJoinRoomByNameButton()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            PhotonManager.Instance.JoinRoomByName(roomNameInput.text);
        }
    }

    public void OnJoinRandomButton()
    {
        PhotonManager.Instance.JoinRandomRoomSafe();
    }

    private void OnRefreshButton()
    {
        Debug.Log("Refreshing room list...");
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        if (!IsMenuScene()) return; 

        ClearRoomList();
        int totalPlayers = 0;

        foreach (var room in roomList)
        {
            if (room.IsOpen && room.IsVisible)
            {
                GameObject roomItem = Instantiate(roomListItemPrefab, roomListParent);

                RoomItemButton itemScript = roomItem.GetComponent<RoomItemButton>();
                if (itemScript != null)
                {
                    itemScript.SetupRoom(room.Name, room.PlayerCount, room.MaxPlayers);
                }
                else
                {
                    TMP_Text[] texts = roomItem.GetComponentsInChildren<TMP_Text>();
                    if (texts.Length > 0) texts[0].text = room.Name;
                    if (texts.Length > 1) texts[1].text = $"{room.PlayerCount}/{room.MaxPlayers}";

                    Button btn = roomItem.GetComponent<Button>();
                    if (btn != null)
                    {
                        string roomName = room.Name;
                        btn.onClick.AddListener(() => PhotonManager.Instance.JoinRoomByName(roomName));
                    }
                }

                roomListItems.Add(roomItem);
                totalPlayers += room.PlayerCount;
            }
        }

        UpdateRoomListUI();
    }

    private void ClearRoomList()
    {
        foreach (var item in roomListItems)
        {
            if (item != null)
                Destroy(item);
        }
        roomListItems.Clear();
    }

    private void UpdateRoomListUI()
    {
        if (roomCountText != null)
            roomCountText.text = $"Rooms: {roomListItems.Count}";

        if (playerCountText != null)
        {
            int totalPlayers = 0;
            playerCountText.text = $"Players Online: {totalPlayers}";
        }
    }
}