using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameTagManager : MonoBehaviourPun
{
    [Header("Configuration")]
    [SerializeField] private PlayerLabelConfig labelConfig;

    public static GameTagManager Instance { get; private set; }

    public void SetPlayerTag(Photon.Realtime.Player player, string tagName)
    {
        var tagConfig = labelConfig.GetTagConfig(tagName);

        Hashtable props = new Hashtable();
        props["playerTag"] = tagName;
        player.SetCustomProperties(props);
    }

    public void AssignRandomTags()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var players = PhotonNetwork.PlayerList;
        if (players.Length == 0) return;

        var killerTag = System.Array.Find(labelConfig.availableTags, tag => tag.tagName.ToLower() == "killer");
        var runnerTag = System.Array.Find(labelConfig.availableTags, tag => tag.tagName.ToLower() == "runner");

        int killerIndex = Random.Range(0, players.Length);
        SetPlayerTag(players[killerIndex], killerTag.tagName);

        for (int i = 0; i < players.Length; i++)
        {
            if (i != killerIndex)
            {
                SetPlayerTag(players[i], runnerTag.tagName);
            }
        }
    }

    public void SetAllPlayersTag(string tagName)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            SetPlayerTag(player, tagName);
        }
    }
}