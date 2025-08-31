using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;

    [SerializeField] private string gameSceneName;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ConnectToPhoton(string nickname, int skinIndex)
    {
        PhotonNetwork.NickName = nickname;

        var customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties["playerSkin"] = skinIndex;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public void JoinLobby()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.JoinLobby();
    }

    public void JoinOrCreateRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
            roomName = "Room" + Random.Range(0, 1000);

        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        JoinLobby();
    }
    public void JoinRandomRoomSafe()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.LogWarning("No se puede unirse a sala aleatoria: espera a estar en el lobby");
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Entered Lobby");
        UIManager.Instance.ShowLobbyPanel();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        JoinOrCreateRoom("");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        SceneManager.LoadScene(gameSceneName);
    }


}
