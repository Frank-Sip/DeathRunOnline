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

    private int aliveRunnersCount = 0;
    private bool gameEnded = false;
    private bool matchStarted = false;
    
    public int ALiveRunnersCount => aliveRunnersCount;

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
            photonView.RPC("RPC_ShowVictory", RpcTarget.All, killer.NickName, "Killer");
        }
    }

    public void ProcessRunnerVictory(string runnerNickname)
    {
        if (gameEnded) return;

        Debug.Log($"GameManager: Procesando victoria de Runner: {runnerNickname}");
        photonView.RPC("RPC_ShowVictory", RpcTarget.All, runnerNickname, "Runner");
    }

    [PunRPC]
    private void RPC_ShowVictory(string winnerNickname, string winnerType)
    {
        if (gameEnded) return;

        gameEnded = true;
        matchStarted = false;

        if (victoryCanvas != null)
        {
            victoryCanvas.SetActive(true);
            if (victoryText != null)
            {
                victoryText.text = $"{winnerNickname} Ganó!";
            }
        }

        Debug.Log($"¡{winnerNickname} ({winnerType}) ha ganado la partida!");
        StartCoroutine(LoadMainMenuAfterDelay());
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
            photonView.RPC("RPC_DecrementRunnerCount", RpcTarget.MasterClient);
            Debug.Log($"Runner {otherPlayer.NickName} se desconectó.");
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

    public bool HasGameEnded() => gameEnded;
}