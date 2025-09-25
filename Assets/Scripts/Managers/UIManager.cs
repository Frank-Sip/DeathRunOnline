using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Button backToNicknameButton;

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

    private Coroutine createRoomRoutine;
    private Coroutine joinRoomByNameRoutine;
    private Coroutine joinOrCreateRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"UIManager Start in scene: {currentScene}");

        if (IsMenuScene(currentScene))
        {
            RestoreCursorState();
            InitializeMenuUI();
            if (PhotonNetwork.IsConnected)
            {
                StartCoroutine(HandleReturnFromGame());
            }
        }
        else
        {
            HideAllCanvases();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private IEnumerator HandleReturnFromGame()
    {
        yield return null; 

        if (PhotonNetwork.InLobby)
        {
            ShowLobbyPanel();
            Debug.Log("Automatically showing lobby after returning from game");
        }
        else
        {
            Debug.Log("Not in lobby after game, returning to nickname screen");
            OnBackToNickname();
        }
    }

    private void RestoreCursorState()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("Cursor state restored - Visible and unlocked");
    }

    private bool IsMenuScene(string sceneName = null)
    {
        if (string.IsNullOrEmpty(sceneName))
            sceneName = SceneManager.GetActiveScene().name;

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

    private void HideAllCanvases()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
    }

    private void InitializeUI()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
    }

    private void SetupButtons()
    {
        if (connectionButton != null)
            connectionButton.onClick.AddListener(OnConnectButton);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButton);

        if (nameInput != null)
        {
            nameInput.onValueChanged.AddListener(VerifyName);
            nameInput.onSubmit.AddListener(OnInputSubmit);
        }

        if (roomNameInput != null)
        {
            roomNameInput.onSubmit.AddListener(OnRoomNameSubmit);
        }

        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(OnCreateRoomButton);

        if (joinRoomButton != null)
            joinRoomButton.onClick.AddListener(OnJoinRoomByNameButton);

        if (joinRandomButton != null)
            joinRandomButton.onClick.AddListener(OnJoinRandomButton);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshButton);

        if (backToNicknameButton != null)
            backToNicknameButton.onClick.AddListener(OnBackToNickname);
    }

    private void OnBackToNickname()
    {
        Debug.Log("Returning to nickname screen");

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);

        RestoreCursorState();

        if (connectionButton != null)
            connectionButton.interactable = true;
        if (continueButton != null)
            continueButton.interactable = !string.IsNullOrWhiteSpace(nickname);
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

    private void OnInputSubmit(string _)
    {
        if (continueButton != null && continueButton.interactable)
        {
            OnContinueButton();
        }
    }

    private void OnRoomNameSubmit(string _)
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            OnCreateRoomButton();
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
        if (!IsMenuScene())
        {
            Debug.LogWarning("Cannot show lobby panel in non-menu scene");
            return;
        }

        RestoreCursorState();

        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(true);
            Debug.Log("Lobby panel activated");
        }
        else
        {
            Debug.LogError("Lobby panel reference is null");
        }

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }

        UpdateRoomListUI();
    }

    #region Photon Callbacks

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("Room properties updated");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");

        if (IsMenuScene())
        {
            OnBackToNickname();
        }
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Left lobby");
        if (IsMenuScene())
        {
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
            if (lobbyPanel != null) lobbyPanel.SetActive(false);
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room");
        RestoreCursorState();
    }

    #endregion

    #region Room Management

    private bool IsMatchmakingReady()
    {
        return PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby;
    }

    private IEnumerator WaitAndCreateRoom(string roomName)
    {
        Debug.Log("[UI] Waiting for matchmaking to be ready (CreateRoom)...");
        yield return new WaitUntil(() => IsMatchmakingReady());
        Debug.Log("[UI] Ready. Create/Join room: " + roomName);
        PhotonManager.Instance.JoinOrCreateRoom(roomName);
        createRoomRoutine = null;
    }

    private IEnumerator WaitAndJoinRoomByName(string roomName)
    {
        Debug.Log("[UI] Waiting for matchmaking to be ready (JoinByName)...");
        yield return new WaitUntil(() => IsMatchmakingReady());
        Debug.Log("[UI] Ready. Join room by name: " + roomName);
        PhotonManager.Instance.JoinRoomByName(roomName);
        joinRoomByNameRoutine = null;
    }

    private IEnumerator WaitAndJoinOrCreate(string roomName)
    {
        Debug.Log("[UI] Waiting for matchmaking to be ready (JoinOrCreate)...");
        yield return new WaitUntil(() => IsMatchmakingReady());
        Debug.Log("[UI] Ready. JoinOrCreate: " + roomName);
        PhotonManager.Instance.JoinOrCreateRoom(roomName);
        joinOrCreateRoutine = null;
    }

    public void OnCreateRoomButton()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : string.Empty;
        if (string.IsNullOrEmpty(roomName))
            roomName = $"{nickname}'s Room";

        if (IsMatchmakingReady())
        {
            PhotonManager.Instance.JoinOrCreateRoom(roomName);
        }
        else if (createRoomRoutine == null)
        {
            Debug.LogWarning("[UI] Matchmaking not ready. Queuing CreateRoom...");
            createRoomRoutine = StartCoroutine(WaitAndCreateRoom(roomName));
        }
    }

    private void OnJoinRoomByNameButton()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : string.Empty;
        if (string.IsNullOrEmpty(roomName)) return;

        if (IsMatchmakingReady())
        {
            PhotonManager.Instance.JoinRoomByName(roomName);
        }
        else if (joinRoomByNameRoutine == null)
        {
            Debug.LogWarning("[UI] Matchmaking not ready. Queuing JoinRoomByName...");
            joinRoomByNameRoutine = StartCoroutine(WaitAndJoinRoomByName(roomName));
        }
    }

    public void OnJoinRandomButton()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : string.Empty;

        if (IsMatchmakingReady())
        {
            PhotonManager.Instance.JoinOrCreateRoom(roomName);
        }
        else if (joinOrCreateRoutine == null)
        {
            Debug.LogWarning("[UI] Matchmaking not ready. Queuing JoinOrCreate...");
            joinOrCreateRoutine = StartCoroutine(WaitAndJoinOrCreate(roomName));
        }
    }

    private void OnRefreshButton()
    {
        Debug.Log("Refreshing room list...");
    }

    #endregion

    #region Room List Management

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
                        string rName = room.Name;
                        btn.onClick.AddListener(() => PhotonManager.Instance.JoinRoomByName(rName));
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
            foreach (var roomItem in roomListItems)
            {
                if (roomItem != null)
                {
                    RoomItemButton itemScript = roomItem.GetComponent<RoomItemButton>();
                    if (itemScript != null)
                    {
                        totalPlayers += itemScript.PlayerCount;
                    }
                }
            }
            playerCountText.text = $"Players Online: {totalPlayers}";
        }
    }

    #endregion
}