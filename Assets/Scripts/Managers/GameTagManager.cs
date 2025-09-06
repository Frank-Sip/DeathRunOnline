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

        Debug.Log($"Tag '{tagName}' asignado a {player.NickName}");
    }

    public void AssignRandomTags()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Solo el MasterClient puede asignar tags");
            return;
        }

        var players = PhotonNetwork.PlayerList;
        if (players.Length == 0)
        {
            Debug.LogWarning("No hay jugadores en la sala");
            return;
        }

        var killerTag = System.Array.Find(labelConfig.availableTags, tag => tag.tagName.ToLower() == "killer");
        var runnerTag = System.Array.Find(labelConfig.availableTags, tag => tag.tagName.ToLower() == "runner");

        if (killerTag == null || runnerTag == null)
        {
            Debug.LogError("No se encontraron las configuraciones de tags 'killer' o 'runner'");
            return;
        }
        int killerIndex = Random.Range(0, players.Length);

        Debug.Log($"Asignando tags a {players.Length} jugadores...");

        for (int i = 0; i < players.Length; i++)
        {
            if (i == killerIndex)
            {
                SetPlayerTag(players[i], killerTag.tagName);
            }
            else
            {
                SetPlayerTag(players[i], runnerTag.tagName);
            }
        }

        Debug.Log("Tags asignados correctamente");
    }

    public void SetAllPlayersTag(string tagName)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Solo el MasterClient puede cambiar todos los tags");
            return;
        }

        foreach (var player in PhotonNetwork.PlayerList)
        {
            SetPlayerTag(player, tagName);
        }

        Debug.Log($"Todos los jugadores ahora tienen el tag: {tagName}");
    }
    public string GetPlayerTag(Photon.Realtime.Player player)
    {
        if (player.CustomProperties.TryGetValue("playerTag", out object tagValue))
        {
            return tagValue.ToString();
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
