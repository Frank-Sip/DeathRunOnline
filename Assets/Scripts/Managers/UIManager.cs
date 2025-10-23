using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Canvas References")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject loadingPanel;

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

    [Header("Feedback System")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float feedbackDisplayTime = 3f;

    private const string nickNameKey = "playerNickname";
    private const string skinKey = "playerSkin";

    private string nickname;
    private int selectedSkinIndex = 0;
    private List<GameObject> roomListItems = new List<GameObject>();

    private Coroutine createRoomRoutine;
    private Coroutine joinRoomByNameRoutine;
    private Coroutine joinOrCreateRoutine;
    private Coroutine feedbackRoutine;

    #region Unity Lifecycle
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
            SubscribeToPhotonEvents();

            if (PhotonManager.Instance != null && PhotonManager.Instance.IsConnected())
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
        UnsubscribeFromPhotonEvents();
    }
    #endregion

    #region Event Subscription 
    private void SubscribeToPhotonEvents()
    {
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.OnLobbyJoined += HandleLobbyJoined;
            PhotonManager.Instance.OnRoomListUpdated += HandleRoomListUpdated;
            PhotonManager.Instance.OnPhotonDisconnected += HandleDisconnected;
            PhotonManager.Instance.OnLobbyLeft += HandleLeftLobby;
            PhotonManager.Instance.OnRoomLeft += HandleLeftRoom;
            PhotonManager.Instance.OnRoomPropertiesChanged += HandleRoomPropertiesUpdate;
            PhotonManager.Instance.OnRoomCreationFailed += HandleRoomCreationFailed;
            PhotonManager.Instance.OnRoomJoinFailed += HandleRoomJoinFailed;
            PhotonManager.Instance.OnMasterServerConnected += HandleMasterServerConnected;
            PhotonManager.Instance.OnRoomJoined += HandleRoomJoined;
        }
    }

    private void UnsubscribeFromPhotonEvents()
    {
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.OnLobbyJoined -= HandleLobbyJoined;
            PhotonManager.Instance.OnRoomListUpdated -= HandleRoomListUpdated;
            PhotonManager.Instance.OnPhotonDisconnected -= HandleDisconnected;
            PhotonManager.Instance.OnLobbyLeft -= HandleLeftLobby;
            PhotonManager.Instance.OnRoomLeft -= HandleLeftRoom;
            PhotonManager.Instance.OnRoomPropertiesChanged -= HandleRoomPropertiesUpdate;
            PhotonManager.Instance.OnRoomCreationFailed -= HandleRoomCreationFailed;
            PhotonManager.Instance.OnRoomJoinFailed -= HandleRoomJoinFailed;
            PhotonManager.Instance.OnMasterServerConnected -= HandleMasterServerConnected;
            PhotonManager.Instance.OnRoomJoined -= HandleRoomJoined;
        }
    }
    #endregion

    #region Photon Event Handlers - REEMPLAZAN LOS CALLBACKS

    private void HandleLobbyJoined()
    {
        Debug.Log("UI: Lobby joined event received");
        ShowLobbyPanel();
    }

    private void HandleRoomListUpdated(List<RoomInfo> roomList)
    {
        Debug.Log($"UI: Room list updated with {roomList.Count} rooms");
        UpdateRoomList(roomList);
    }

    private void HandleDisconnected(DisconnectCause cause)
    {
        Debug.Log($"UI: Disconnected event received: {cause}");
        
        // Show friendly disconnect message
        ShowFeedbackMessage(GetDisconnectMessage(cause));
        
        if (IsMenuScene())
        {
            OnBackToNickname();
        }
    }

    private void HandleLeftLobby()
    {
        Debug.Log("UI: Left lobby event received");
        if (IsMenuScene())
        {
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
            if (lobbyPanel != null) lobbyPanel.SetActive(false);
        }
    }

    private void HandleLeftRoom()
    {
        Debug.Log("UI: Left room event received");
        RestoreCursorState();
    }

    private void HandleRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("UI: Room properties updated");
    }

    private void HandleRoomCreationFailed(string message)
    {
        Debug.LogError($"UI: Room creation failed - {message}");
        ShowFeedbackMessage($"Error al crear sala: {message}");
    }

    private void HandleRoomJoinFailed(string message)
    {
        Debug.LogError($"UI: Room join failed - {message}");
        ShowFeedbackMessage($"Error al unirse a la sala: {message}");
    }

    private void HandleMasterServerConnected()
    {
        Debug.Log("UI: Connected to Master Server");
        ShowFeedbackMessage("Conectado al servidor");
    }

    private void HandleRoomJoined(string roomName)
    {
        Debug.Log($"UI: Joined room - {roomName}");
        ShowFeedbackMessage($"Uniéndose a: {roomName}");
    }
    #endregion

    #region Initialization
    private IEnumerator HandleReturnFromGame()
    {
        yield return null;

        if (PhotonManager.Instance != null && PhotonManager.Instance.IsInLobby())
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

        return sceneName == "MainMenu" || sceneName == "Menu" || sceneName == "Lobby" || sceneName == "Loading";
    }

    private void InitializeMenuUI()
    {
        InitializeUI();
        SetupButtons();
        SetupSkinButtons();
        SelectSkin(0);

        // Initialize feedback panel as disabled
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

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
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
    }

    private void InitializeUI()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }
    #endregion

    #region Button Setup
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

        if (PhotonManager.Instance != null && PhotonManager.Instance.IsConnected())
        {
            PhotonManager.Instance.DisconnectFromPhoton();
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
    #endregion

    #region Skin Selection
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
    #endregion

    #region Input Handlers
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
        if (loadingPanel != null) loadingPanel.SetActive(true);

        PhotonManager.Instance.ConnectToPhoton(nickname, selectedSkinIndex);
    }
    #endregion

    #region Lobby Panel
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
    #endregion

    #region Room Management

    private IEnumerator WaitAndCreateRoom(string roomName)
    {
        Debug.Log("[UI] Waiting for matchmaking to be ready (CreateRoom)...");
        yield return new WaitUntil(() => PhotonManager.Instance.IsMatchmakingReady());
        Debug.Log("[UI] Ready. Create/Join room: " + roomName);
        PhotonManager.Instance.JoinOrCreateRoom(roomName);
        createRoomRoutine = null;
    }

    private IEnumerator WaitAndJoinRoomByName(string roomName)
    {
        Debug.Log("[UI] Waiting for matchmaking to be ready (JoinByName)...");
        yield return new WaitUntil(() => PhotonManager.Instance.IsMatchmakingReady());
        Debug.Log("[UI] Ready. Join room by name: " + roomName);
        PhotonManager.Instance.JoinRoomByName(roomName);
        joinRoomByNameRoutine = null;
    }

    private IEnumerator WaitAndJoinOrCreate(string roomName)
    {
        Debug.Log("[UI] Waiting for matchmaking to be ready (JoinOrCreate)...");
        yield return new WaitUntil(() => PhotonManager.Instance.IsMatchmakingReady());
        Debug.Log("[UI] Ready. JoinOrCreate: " + roomName);
        PhotonManager.Instance.JoinOrCreateRoom(roomName);
        joinOrCreateRoutine = null;
    }

    public void OnCreateRoomButton()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : string.Empty;
        if (string.IsNullOrEmpty(roomName))
            roomName = $"{nickname}'s Room";

        if (PhotonManager.Instance.IsMatchmakingReady())
        {
            ShowFeedbackMessage($"Creando sala: {roomName}");
            PhotonManager.Instance.JoinOrCreateRoom(roomName);
        }
        else if (createRoomRoutine == null)
        {
            Debug.LogWarning("[UI] Matchmaking not ready. Queuing CreateRoom...");
            ShowFeedbackMessage("Esperando conexión...");
            createRoomRoutine = StartCoroutine(WaitAndCreateRoom(roomName));
        }
    }

    private void OnJoinRoomByNameButton()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : string.Empty;
        if (string.IsNullOrEmpty(roomName))
        {
            ShowFeedbackMessage("Ingresa un nombre de sala");
            return;
        }

        if (PhotonManager.Instance.IsMatchmakingReady())
        {
            ShowFeedbackMessage($"Uniéndose a: {roomName}");
            PhotonManager.Instance.JoinRoomByName(roomName);
        }
        else if (joinRoomByNameRoutine == null)
        {
            Debug.LogWarning("[UI] Matchmaking not ready. Queuing JoinRoomByName...");
            ShowFeedbackMessage("Esperando conexión...");
            joinRoomByNameRoutine = StartCoroutine(WaitAndJoinRoomByName(roomName));
        }
    }

    public void OnJoinRandomButton()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : string.Empty;

        if (PhotonManager.Instance.IsMatchmakingReady())
        {
            ShowFeedbackMessage("Buscando sala...");
            PhotonManager.Instance.JoinOrCreateRoom(roomName);
        }
        else if (joinOrCreateRoutine == null)
        {
            Debug.LogWarning("[UI] Matchmaking not ready. Queuing JoinOrCreate...");
            ShowFeedbackMessage("Esperando conexión...");
            joinOrCreateRoutine = StartCoroutine(WaitAndJoinOrCreate(roomName));
        }
    }

    private void OnRefreshButton()
    {
        Debug.Log("Refreshing room list...");
        ShowFeedbackMessage("Actualizando lista de salas...");
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

    #region Feedback System
    public void ShowFeedbackMessage(string message)
    {
        if (feedbackPanel == null || feedbackText == null)
        {
            Debug.LogWarning("Feedback panel or text not assigned in UIManager!");
            return;
        }
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        feedbackRoutine = StartCoroutine(DisplayFeedbackRoutine(message));
    }

    private IEnumerator DisplayFeedbackRoutine(string message)
    {
        feedbackPanel.SetActive(true);
        feedbackText.text = message;
        yield return new WaitForSeconds(feedbackDisplayTime);
        feedbackPanel.SetActive(false);
        feedbackRoutine = null;
        Debug.Log("[Feedback] Message hidden");
    }
    private string GetDisconnectMessage(DisconnectCause cause)
    {
        switch (cause)
        {
            case DisconnectCause.None:
                return "Desconectado del servidor";
            
            case DisconnectCause.ExceptionOnConnect:
            case DisconnectCause.DnsExceptionOnConnect:
            case DisconnectCause.ServerAddressInvalid:
                return "No se pudo conectar al servidor. Verifica tu conexión";
            
            case DisconnectCause.Exception:
            case DisconnectCause.SendException:
            case DisconnectCause.ReceiveException:
                return "Error de conexión. Por favor intenta de nuevo";
            
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
                return "Tiempo de espera agotado. Verifica tu conexión";
            
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                return "Desconectado por el servidor";
            
            case DisconnectCause.InvalidAuthentication:
            case DisconnectCause.CustomAuthenticationFailed:
            case DisconnectCause.AuthenticationTicketExpired:
                return "Error de autenticación. Reinicia el juego";
            
            case DisconnectCause.MaxCcuReached:
                return "Servidor lleno. Intenta más tarde";
            
            case DisconnectCause.InvalidRegion:
                return "Región no disponible";
            
            case DisconnectCause.OperationNotAllowedInCurrentState:
                return "Operación no permitida en este momento";
            
            case DisconnectCause.DisconnectByClientLogic:
                return "Desconectado";
            
            case DisconnectCause.DisconnectByOperationLimit:
                return "Demasiadas operaciones. Espera un momento";
            
            case DisconnectCause.DisconnectByDisconnectMessage:
                return "Desconectado del servidor";
            
            case DisconnectCause.ApplicationQuit:
                return "Aplicación cerrada";
            
            default:
                return "Desconectado del servidor";
        }
    }
    public void HideFeedback()
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);
    }
    #endregion
}