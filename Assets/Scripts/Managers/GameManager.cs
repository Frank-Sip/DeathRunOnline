using System.Collections;
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
        Debug.Log($"Match started. Waiting for runner count...");
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
        if (!matchStarted || gameEnded) return;

        aliveRunnersCount--;
        Debug.Log($"----Runner died. Remaining runners: {aliveRunnersCount}");

        CheckKillerWin();
    }

    private void CheckKillerWin()
    {
        if (gameEnded) return;

        if (aliveRunnersCount <= 0)
        {
            Player killer = GameTagManager.Instance.GetKillerPlayer();
            if (killer != null)
            {
                Debug.Log($"----GameManager: Procesando victoria de Killer: {killer.NickName}");
                float elapsedTime = StartGameButton.GetElapsedTime();
                photonView.RPC("RPC_ShowVictory", RpcTarget.All, killer.NickName, "Killer", elapsedTime);
            }
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

        // Enviar score a LootLocker (solo el ganador local)
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
        if (!matchStarted || gameEnded) return;

        if (IsPlayerRunner(otherPlayer))
        {
            photonView.RPC("RPC_DecrementRunnerCount", RpcTarget.All);
            Debug.Log($"Runner {otherPlayer.NickName} se desconectó. Decrementando contador.");
        }
        CheckKillerWin();
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