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

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void ConnectToPhoton(string nickname, int skinIndex)
    {
        PhotonNetwork.NickName = nickname;
        var customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties["playerSkin"] = skinIndex;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void JoinLobby()
    {

        Debug.Log("Joining Lobby...");
        PhotonNetwork.JoinLobby();
        
    }

    public void JoinOrCreateRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
            roomName = "Room_" + Random.Range(1000, 9999);

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true
        };

        Debug.Log($"Joining or Creating room: {roomName}");
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public void JoinRoomByName(string roomName)
    {
        if (!string.IsNullOrEmpty(roomName))
        {
            Debug.Log($"Joining room: {roomName}");
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    public void JoinRandomRoom()
    {
        Debug.Log("Joining random room...");
        PhotonNetwork.JoinRandomRoom();
    }

    public void JoinRandomRoomSafe()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.LogWarning("Cannot join random room: Not in lobby");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby Successfully");
        ClearLocalPlayerTag();
        UIManager.Instance.ShowLobbyPanel();
    }
    private void ClearLocalPlayerTag()
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["playerTag"] = null;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.Log($"Local player tag cleared for: {PhotonNetwork.LocalPlayer.NickName}");
        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"Room list updated. Count: {roomList.Count}");
        UpdateCachedRoomList(roomList);
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateRoomList(cachedRoomList);
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (var room in roomList)
        {
            if (room.RemovedFromList)
            {
                cachedRoomList.RemoveAll(r => r.Name == room.Name);
            }
            else
            {
                int index = cachedRoomList.FindIndex(r => r.Name == room.Name);
                if (index != -1)
                {
                    cachedRoomList[index] = room;
                }
                else
                {
                    cachedRoomList.Add(room);
                }
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join Random Failed, creating new room");
        JoinOrCreateRoom("");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create Room Failed: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join Room Failed: {message}");
    }
}