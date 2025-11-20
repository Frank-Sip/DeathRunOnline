using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("Victory UI")]
    [SerializeField] private GameObject victoryCanvas;
    [SerializeField] private TMP_Text victoryText;

    [Header("Game Settings")]
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("LootLocker Settings")]
    [SerializeField] private string leaderboardKey = "leaderboard_key2";

    private int aliveRunnersCount = 0;
    public int ALiveRunnersCount => aliveRunnersCount;
    private bool gameEnded = false;
    private bool matchStarted = false;
    
    private HashSet<int> runnersCache = new HashSet<int>();

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

        if (victoryCanvas != null)
            victoryCanvas.SetActive(false);
    }

    public void StartMatch()
    {
        gameEnded = false;
        matchStarted = true;
        runnersCache.Clear();
        
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("playerTag", out object tagValue))
            {
                string tag = tagValue.ToString();
                if (tag.ToLower() == "runner")
                {
                    runnersCache.Add(player.ActorNumber);
                    Debug.Log($"[GameManager] Runner encontrado y agregado al cache: {player.NickName} (ActorNumber: {player.ActorNumber})");
                }
            }
        }
        
        Debug.Log($"[GameManager] Match started. Runners en cache: {runnersCache.Count}");
    }

    public void AddRunnerToCache(int actorNumber)
    {
        runnersCache.Add(actorNumber);
        Debug.Log($"[GameManager] Runner agregado al cache: ActorNumber {actorNumber}. Total: {runnersCache.Count}");
    }

    public void RemoveRunnerFromCache(int actorNumber)
    {
        if (runnersCache.Remove(actorNumber))
        {
            Debug.Log($"[GameManager] Runner removido del cache: ActorNumber {actorNumber}. Restantes: {runnersCache.Count}");
        }
    }

    public bool IsRunnerInCache(int actorNumber)
    {
        return runnersCache.Contains(actorNumber);
    }

    [PunRPC]
    public void RPC_IncrementRunnerCount()
    {
        aliveRunnersCount++;
        Debug.Log($"----Runner count increased to: {aliveRunnersCount}");
    }

    [PunRPC]
    public void RPC_DecrementRunnerCount()
    {
        if (!matchStarted || gameEnded)
        {
            Debug.Log($"[GameManager] RPC_DecrementRunnerCount ignorado - matchStarted: {matchStarted}, gameEnded: {gameEnded}");
            return;
        }

        aliveRunnersCount--;
        Debug.Log($"[GameManager] ---- Runner count decrementado. Runners vivos: {aliveRunnersCount}");

        CheckKillerWin();
    }

    private void CheckKillerWin()
    {
        Debug.Log($"[GameManager] CheckKillerWin - gameEnded: {gameEnded}, aliveRunnersCount: {aliveRunnersCount}");
        
        if (gameEnded) return;

        if (aliveRunnersCount <= 0)
        {
            Debug.Log($"[GameManager] ¡TODOS LOS RUNNERS ELIMINADOS! Buscando Killer...");
            Player killer = GameTagManager.Instance.GetKillerPlayer();
            if (killer != null)
            {
                Debug.Log($"[GameManager] ¡¡¡VICTORIA DEL KILLER!!! Ganador: {killer.NickName}");
                float elapsedTime = StartGameButton.GetElapsedTime();
                photonView.RPC("RPC_ShowVictory", RpcTarget.All, killer.NickName, "Killer", elapsedTime);
            }
            else
            {
                Debug.LogError($"[GameManager] ERROR: No se encontró al Killer!");
            }
        }
        else
        {
            Debug.Log($"[GameManager] Aún quedan {aliveRunnersCount} runners. El juego continúa.");
        }
    }

    public void ProcessRunnerVictory(string runnerNickname)
    {
        if (gameEnded) return;

        Debug.Log($"GameManager: Procesando victoria de Runner: {runnerNickname}");
        float elapsedTime = StartGameButton.GetElapsedTime();
        photonView.RPC("RPC_ShowVictory", RpcTarget.All, runnerNickname, "Runner", elapsedTime);
    }

    [PunRPC]
    private void RPC_ShowVictory(string winnerNickname, string winnerType, float finalTime)
    {
        if (gameEnded) return;

        gameEnded = true;
        matchStarted = false;

        if (victoryCanvas != null)
        {
            victoryCanvas.SetActive(true);
            if (victoryText != null)
            {
                int seconds = Mathf.FloorToInt(finalTime);
                int milliseconds = Mathf.FloorToInt((finalTime - seconds) * 1000);
                victoryText.text = $"{winnerNickname} Ganó!\nRol: {winnerType}\nTiempo: {seconds}s {milliseconds}ms";
            }
        }

        Debug.Log($"¡{winnerNickname} ({winnerType}) ha ganado la partida en {finalTime:F3} segundos!");

        if (PhotonNetwork.LocalPlayer.NickName == winnerNickname)
        {
            SubmitScoreToLeaderboard(finalTime, winnerType);
        }

        StartCoroutine(LoadMainMenuAfterDelay());
    }

    private void SubmitScoreToLeaderboard(float time, string role)
    {
        int scoreInMilliseconds = Mathf.FloorToInt(time * 1000);

        Debug.Log($"[GameManager] Enviando score a LootLocker: {scoreInMilliseconds}ms ({time:F3}s) como {role}");

        LeaderboardService.SubmitScoreWithMetadata(scoreInMilliseconds, role, leaderboardKey, success =>
        {
            if (success)
            {
                Debug.Log($"[GameManager] ¡Score enviado exitosamente! Tiempo: {time:F3}s - Rol: {role}");
            }
            else
            {
                Debug.LogError($"[GameManager] Error al enviar el score a LootLocker");
            }
        });
    }

    private IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        ClearAllPlayerTags();

        PhotonNetwork.LoadLevel(mainMenuSceneName);
    }

    private void ClearAllPlayerTags()
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            photonView.RPC("RPC_ClearPlayerTag", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    private void RPC_ClearPlayerTag(int actorNumber)
    {
        Photon.Realtime.Player player = PhotonNetwork.CurrentRoom?.GetPlayer(actorNumber);
        if (player != null)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["playerTag"] = null;
            player.SetCustomProperties(props);
            Debug.Log($"Tag cleared for player: {player.NickName}");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[GameManager] OnPlayerLeftRoom - Player: {otherPlayer.NickName}, ActorNumber: {otherPlayer.ActorNumber}");
        Debug.Log($"[GameManager] IsMasterClient: {PhotonNetwork.IsMasterClient}, matchStarted: {matchStarted}, gameEnded: {gameEnded}");
        Debug.Log($"[GameManager] Cache actual de runners: [{string.Join(", ", runnersCache)}]");
        
        if (!matchStarted || gameEnded)
        {
            Debug.Log($"[GameManager] Ignorado - matchStarted: {matchStarted}, gameEnded: {gameEnded}");
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"[GameManager] Este cliente NO es MasterClient. Ignorando desconexión.");
            return;
        }

        bool isRunner = IsRunnerInCache(otherPlayer.ActorNumber);
        Debug.Log($"[GameManager] IsRunnerInCache({otherPlayer.ActorNumber}): {isRunner}");
        
        if (isRunner)
        {
            RemoveRunnerFromCache(otherPlayer.ActorNumber);
            photonView.RPC("RPC_DecrementRunnerCount", RpcTarget.All);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        
        if (!matchStarted || gameEnded) return;
        
        Debug.Log($"MasterClient cambió a: {newMasterClient.NickName}. Verificando victoria del Killer.");
        CheckKillerWin();
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

    public bool HasGameEnded() => gameEnded;
}