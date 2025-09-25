using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject killerVictoryCanvas;
    [SerializeField] private TMP_Text killerVictoryText;
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private readonly HashSet<int> aliveRunnerActors = new HashSet<int>();
    private bool gameEnded = false;
    private bool matchStarted = false;

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

        if (killerVictoryCanvas != null)
            killerVictoryCanvas.SetActive(false);
    }

    private void Update()
    {
        if (!matchStarted || gameEnded) return;
        
        RefreshAliveRunners();
        
        if (aliveRunnerActors.Count == 0)
        {
            CheckKillerVictory();
        }
    }

    private void RefreshAliveRunners()
    {
        aliveRunnerActors.Clear();
        
        PlayerModel[] allPlayers = FindObjectsOfType<PlayerModel>();
        
        foreach (PlayerModel player in allPlayers)
        {
            if (player.isAlive && IsPlayerRunner(player.PhotonView.Owner))
            {
                aliveRunnerActors.Add(player.PhotonView.Owner.ActorNumber);
            }
        }
    }

    private bool IsPlayerRunner(Player player)
    {
        if (player.CustomProperties.TryGetValue("playerTag", out object tagValue))
        {
            string playerTag = tagValue.ToString();
            return playerTag.ToLower() == "runner";
        }
        return false;
    }

    public void StartMatch()
    {
        aliveRunnerActors.Clear();
        gameEnded = false;
        matchStarted = true;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (IsPlayerRunner(player))
            {
                aliveRunnerActors.Add(player.ActorNumber);
            }
        }

        Debug.Log($"Match started with {aliveRunnerActors.Count} runners");
    }

    private void CheckKillerVictory()
    {
        if (gameEnded) return;

        Player killer = GameTagManager.Instance.GetKillerPlayer();
        if (killer != null)
        {
            photonView.RPC("RPC_ShowKillerVictory", RpcTarget.All, killer.NickName);
        }
    }

    [PunRPC]
    private void RPC_ShowKillerVictory(string killerNickname)
    {
        if (gameEnded) return;

        gameEnded = true;
        matchStarted = false;

        if (killerVictoryCanvas != null)
        {
            killerVictoryCanvas.SetActive(true);

            if (killerVictoryText != null)
            {
                killerVictoryText.text = $"{killerNickname} Ganó!";
            }
        }

        Debug.Log($"¡{killerNickname} (Killer) ha ganado la partida!");
        StartCoroutine(LoadMainMenuAfterDelay());
    }

    private IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        PhotonNetwork.LoadLevel(mainMenuSceneName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!matchStarted || gameEnded) return;

        if (IsPlayerRunner(otherPlayer))
        {
            aliveRunnerActors.Remove(otherPlayer.ActorNumber);
            Debug.Log($"Runner {otherPlayer.NickName} se desconectó. Runners restantes: {aliveRunnerActors.Count}");
        }
    }

    public bool HasGameEnded() => gameEnded;
}