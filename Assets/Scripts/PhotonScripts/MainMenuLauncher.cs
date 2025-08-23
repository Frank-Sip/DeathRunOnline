using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuLauncher : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameSceneName;
    [SerializeField] private TMP_InputField InputField;
    [SerializeField] private Button connectionButton;
    
    private const string nickNameKey = "playerNickname";
    private string nickname;
    
    private void Start()
    {
        connectionButton.onClick.AddListener(HandleConnectButton);
        InputField.onSubmit.AddListener(OnInputSubmit);
        InputField.onValueChanged.AddListener(VerifyName);
        VerifyName(InputField.text);
    }

    private void VerifyName(string name)
    {
        connectionButton.interactable = !string.IsNullOrWhiteSpace(name);
        nickname = name;
    }
    
    private void HandleConnectButton()
    {
        PlayerPrefs.SetString(nickNameKey, nickname);
        
        PhotonNetwork.NickName = nickname;
        print(nickname + "is trying to connect to the room");
        
        PhotonNetwork.ConnectUsingSettings();

        connectionButton.interactable = false;
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
       SceneManager.LoadScene(gameSceneName);
    }
}
