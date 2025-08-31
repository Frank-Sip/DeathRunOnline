using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
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

    [Header("Skin Configuration")]
    [SerializeField] private Button[] skinButtons;
    [SerializeField] private Image skinPreview;
    [SerializeField] private PlayerSkinConfig skinConfig;
    private const string nickNameKey = "playerNickname";
    private const string skinKey = "playerSkin";
    private string nickname;
    private int selectedSkinIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        connectionButton.onClick.AddListener(OnConnectButton);
        continueButton.onClick.AddListener(OnContinueButton);
        nameInput.onValueChanged.AddListener(VerifyName);

        SetupSkinButtons();
        SelectSkin(0);
        VerifyName(nameInput.text);

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

    private void OnContinueButton()
    {
        PlayerPrefs.SetString("playerNickname", nickname);
        PlayerPrefs.SetInt("playerSkin", selectedSkinIndex);
        PlayerPrefs.Save();

        mainMenuCanvas.SetActive(false);
        PhotonManager.Instance.ConnectToPhoton(nickname, selectedSkinIndex);
    }

    private void OnConnectButton()
    {
        connectionButton.interactable = false;
        mainMenuCanvas.SetActive(false);
        PhotonManager.Instance.ConnectToPhoton(nickname, selectedSkinIndex);
    }

    public void ShowLobbyPanel()
    {
        lobbyPanel.SetActive(true);
    }




    // Lobby buttons
    public void CreateRoom()
    {
        PhotonManager.Instance.JoinOrCreateRoom(roomNameInput.text);
    }

    public void JoinRoomByName()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
            PhotonManager.Instance.JoinOrCreateRoom(roomNameInput.text);
    }

    public void JoinRandomRoom()
    {
        PhotonManager.Instance.JoinRandomRoomSafe();
    }
}