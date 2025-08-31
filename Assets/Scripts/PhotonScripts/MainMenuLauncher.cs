using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuLauncher : MonoBehaviourPunCallbacks
{

    [Header("Menu Splash Screen Config")]
    [SerializeField] private string gameSceneName;
    [SerializeField] private TMP_InputField InputField;
    [SerializeField] private Button connectionButton;
    [SerializeField] private Button[] skinButtons;
    [SerializeField] private Image skinPreview;

    [Header("Canvas References")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject lobbyPanel;

    [Header("Skin Configuration")]
    [SerializeField] private PlayerSkinConfig skinConfig;
    private const string nickNameKey = "playerNickname";
    private const string skinKey = "playerSkin";
    private string nickname;
    private int selectedSkinIndex = 0;

    [Header("Lobby UI")]
    [SerializeField] private TMP_InputField roomNameInput;

    private void Start()
    {
        connectionButton.onClick.AddListener(HandleConnectButton);
        InputField.onSubmit.AddListener(OnInputSubmit);
        InputField.onValueChanged.AddListener(VerifyName);

        SetupSkinButtons();
        SelectSkin(0);
        VerifyName(InputField.text);

        mainMenuCanvas.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    private void VerifyName(string name)
    {
        connectionButton.interactable = !string.IsNullOrWhiteSpace(name);
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
        if (skinData.skinIcon != null)
        {
            skinPreview.sprite = skinData.skinIcon;
        }

        for (int i = 0; i < skinButtons.Length; i++)
        {
            skinButtons[i].interactable = i != skinIndex;
        }
    }

    private void HandleConnectButton()
    {
        PlayerPrefs.SetString(nickNameKey, nickname);

        PhotonNetwork.NickName = nickname;

        var customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties[skinKey] = selectedSkinIndex;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

        Debug.Log(nickname + " is trying to connect to the master");

        PhotonNetwork.ConnectUsingSettings();
        connectionButton.interactable = false;

        mainMenuCanvas.SetActive(false);
    }

    private void OnInputSubmit(string name)
    {
        if (connectionButton.interactable)
        {
            HandleConnectButton();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(nickname + " connected to master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Enter lobby");
        lobbyPanel.SetActive(true);
    }

    // ==== Lobby Actions ====

    public void CreateRoom()
    {
        string roomName = string.IsNullOrEmpty(roomNameInput.text) ? "Room:" + Random.Range(0, 1000) : roomNameInput.text;
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void JoinRoomByName()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            PhotonNetwork.JoinRoom(roomNameInput.text);
        }
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No rooms, Creating room");
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom("Random Room" + Random.Range(0, 1000), options);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        SceneManager.LoadScene(gameSceneName);
    }
}
