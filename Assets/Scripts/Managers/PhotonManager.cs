using System;
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

    #region Events
    public event Action OnMasterServerConnected;
    public event Action OnLobbyJoined;
    public event Action<List<RoomInfo>> OnRoomListUpdated;
    public event Action<string> OnRoomJoined;
    public event Action<string> OnRoomCreationFailed;
    public event Action<string> OnRoomJoinFailed;
    public event Action<DisconnectCause> OnPhotonDisconnected;
    public event Action OnLobbyLeft;
    public event Action OnRoomLeft;
    public event Action<ExitGames.Client.Photon.Hashtable> OnRoomPropertiesChanged;
    #endregion

    #region Unity Lifecycle
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
    #endregion

    #region Public API - Connection
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

    public void DisconnectFromPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Disconnecting from Photon...");
            PhotonNetwork.Disconnect();
        }
    }

    public void JoinLobby()
    {
        Debug.Log("Joining Lobby...");
        PhotonNetwork.JoinLobby();
    }

    public void LeaveLobby()
    {
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("Leaving Lobby...");
            PhotonNetwork.LeaveLobby();
        }
    }
    #endregion

    #region Public API - Room Management
    public void JoinOrCreateRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
            roomName = "Room_" + UnityEngine.Random.Range(1000, 9999);

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

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Leaving room...");
            PhotonNetwork.LeaveRoom();
        }
    }
    #endregion

    #region Public API - State Queries
    public bool IsConnected()
    {
        return PhotonNetwork.IsConnected;
    }

    public bool IsInLobby()
    {
        return PhotonNetwork.InLobby;
    }

    public bool IsInRoom()
    {
        return PhotonNetwork.InRoom;
    }

    public bool IsConnectedAndReady()
    {
        return PhotonNetwork.IsConnectedAndReady;
    }

    public bool IsMatchmakingReady()
    {
        return PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby;
    }

    public string GetCurrentRoomName()
    {
        return PhotonNetwork.CurrentRoom?.Name ?? string.Empty;
    }

    public List<RoomInfo> GetCachedRoomList()
    {
        return new List<RoomInfo>(cachedRoomList);
    }
    #endregion

    #region Public API - Player Management
    public void SetLocalPlayerProperty(string key, object value)
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props[key] = value;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    public void ClearLocalPlayerTag()
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["playerTag"] = null;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.Log($"Local player tag cleared for: {PhotonNetwork.LocalPlayer.NickName}");
        }
    }

    public string GetLocalPlayerNickname()
    {
        return PhotonNetwork.NickName;
    }
    #endregion

    #region Public API - Scene Management
    public void LoadGameScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }
    #endregion

    #region PUN Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        JoinLobby();
        OnMasterServerConnected?.Invoke();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby Successfully");
        ClearLocalPlayerTag();
        OnLobbyJoined?.Invoke();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"Room list updated. Count: {roomList.Count}");
        UpdateCachedRoomList(roomList);
        OnRoomListUpdated?.Invoke(cachedRoomList);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join Random Failed, creating new room");
        JoinOrCreateRoom("");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");
        OnRoomJoined?.Invoke(PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create Room Failed: {message}");
        OnRoomCreationFailed?.Invoke(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join Room Failed: {message}");
        OnRoomJoinFailed?.Invoke(message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");
        OnPhotonDisconnected?.Invoke(cause);
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Left lobby");
        OnLobbyLeft?.Invoke();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room");
        OnRoomLeft?.Invoke();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("Room properties updated");
        OnRoomPropertiesChanged?.Invoke(propertiesThatChanged);
    }
    #endregion

    #region Private Helpers
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
    #endregion
}