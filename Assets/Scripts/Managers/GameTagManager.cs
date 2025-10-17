using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameTagManager : MonoBehaviourPun
{
    [Header("Configuration")]
    [SerializeField] private PlayerLabelConfig labelConfig;

    public static GameTagManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerTag(Photon.Realtime.Player player, string tagName)
    {
        var tagConfig = labelConfig.GetTagConfig(tagName);
        Hashtable props = new Hashtable();
        props["playerTag"] = tagName;
        player.SetCustomProperties(props);
    }

    public void AssignRandomTags()
    {
        var players = PhotonNetwork.PlayerList;
        var killerTag = System.Array.Find(labelConfig.availableTags, tag => tag.tagName.ToLower() == "killer");
        var runnerTag = System.Array.Find(labelConfig.availableTags, tag => tag.tagName.ToLower() == "runner");

        int killerIndex = Random.Range(0, players.Length);

        for (int i = 0; i < players.Length; i++)
        {
            if (i == killerIndex)
            {
                SetPlayerTag(players[i], killerTag.tagName);
            }
            else
            {
                SetPlayerTag(players[i], runnerTag.tagName);
                GameManager.Instance.photonView.RPC("RPC_IncrementRunnerCount", RpcTarget.All);
            }
        }

        //Debug.Log($"Tags assigned: 1 Killer, {players.Length - 1} Runners");
    }

    public void SetAllPlayersTag(string tagName)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            SetPlayerTag(player, tagName);
        }
        Debug.Log($"Todos los jugadores ahora tienen el tag: {tagName}");
    }

    public void ClearAllPlayerTags()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            ClearPlayerTag(player);
        }
        Debug.Log("All player tags cleared");
    }

    public void ClearPlayerTag(Photon.Realtime.Player player)
    {
        Hashtable props = new Hashtable();
        props["playerTag"] = null;
        player.SetCustomProperties(props);
    }

    public string GetPlayerTag(Photon.Realtime.Player player)
    {
        if (player.CustomProperties.TryGetValue("playerTag", out object tagValue))
        {
            return tagValue?.ToString();
        }
        return null;
    }

    public bool IsPlayerKiller(Photon.Realtime.Player player)
    {
        string tag = GetPlayerTag(player);
        return tag != null && tag.ToLower() == "killer";
    }

    public Photon.Realtime.Player GetKillerPlayer()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (IsPlayerKiller(player))
            {
                return player;
            }
        }
        return null;
    }
}