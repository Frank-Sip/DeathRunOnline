using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class GameManager2 : MonoBehaviourPunCallbacks
{
    public static GameManager2 Instance;

    [Header("Game Settings")]
    [SerializeField] private int pointsToWin = 5;
    [SerializeField] private string menuSceneName = "MainMenu";

    [Header("Spawn Settings")]
    [SerializeField] private GameObject paddlePrefab;
    [SerializeField] private Transform[] team1SpawnPoints; // Izquierda
    [SerializeField] private Transform[] team2SpawnPoints; // Derecha

    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballSpawnPoint;

    [Header("UI References")]
    [SerializeField] private TMP_Text team1ScoreText;
    [SerializeField] private TMP_Text team2ScoreText;
    [SerializeField] private TMP_Text readyCountText;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;

    private Dictionary<int, GameObject> playerPaddles = new Dictionary<int, GameObject>();
    private GameObject ball;
    private int team1Score = 0;
    private int team2Score = 0;
    private bool gameStarted = false;
    private HashSet<int> readyPlayers = new HashSet<int>();

    private const string READY_PLAYERS_KEY = "ReadyPlayers";
    private const string GAME_STARTED_KEY = "GameStarted";
    private const string TEAM1_SCORE_KEY = "Team1Score";
    private const string TEAM2_SCORE_KEY = "Team2Score";

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

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(menuSceneName);
            return;
        }

        SpawnPaddle();
        UpdateReadyUI();
        ShowWaitingPanel();

        if (PhotonNetwork.IsMasterClient)
        {
            InitializeRoomProperties();
        }
    }

    private void InitializeRoomProperties()
    {
        var props = new ExitGames.Client.Photon.Hashtable
        {
            { GAME_STARTED_KEY, false },
            { TEAM1_SCORE_KEY, 0 },
            { TEAM2_SCORE_KEY, 0 },
            { READY_PLAYERS_KEY, "" }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private void SpawnPaddle()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        int team = GetPlayerTeam(actorNumber);
        int teamIndex = GetTeamPlayerIndex(actorNumber, team);

        Transform spawnPoint = GetSpawnPoint(team, teamIndex);

        GameObject paddle = PhotonNetwork.Instantiate(
            paddlePrefab.name,
            spawnPoint.position,
            spawnPoint.rotation
        );

        var paddleController = paddle.GetComponent<PaddleController>();
        if (paddleController != null)
        {
            paddleController.SetTeam(team);
        }
    }

    private int GetPlayerTeam(int actorNumber)
    {
        // Asignación cíclica: 1->Team1, 2->Team2, 3->Team1, 4->Team2
        return ((actorNumber - 1) % 2) + 1;
    }

    private int GetTeamPlayerIndex(int actorNumber, int team)
    {
        // Índice dentro del equipo (0 o 1)
        return (actorNumber - 1) / 2;
    }

    private Transform GetSpawnPoint(int team, int teamIndex)
    {
        Transform[] spawnPoints = team == 1 ? team1SpawnPoints : team2SpawnPoints;

        if (teamIndex >= spawnPoints.Length)
            teamIndex = spawnPoints.Length - 1;

        return spawnPoints[teamIndex];
    }

    public void OnReadyButtonPressed()
    {
        photonView.RPC("RPC_PlayerReady", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
        readyButton.SetActive(false);
    }

    [PunRPC]
    private void RPC_PlayerReady(int actorNumber)
    {
        readyPlayers.Add(actorNumber);
        UpdateReadyUI();
        CheckStartGame();
    }

    private void UpdateReadyUI()
    {
        if (readyCountText != null)
        {
            readyCountText.text = $"Ready: {readyPlayers.Count}/{PhotonNetwork.CurrentRoom.PlayerCount}";
        }
    }

    private void CheckStartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Verificar que hay al menos un jugador en cada equipo
        bool team1HasPlayer = false;
        bool team2HasPlayer = false;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            int team = GetPlayerTeam(player.ActorNumber);
            if (team == 1) team1HasPlayer = true;
            if (team == 2) team2HasPlayer = true;
        }

        // Verificar que todos los jugadores están listos
        bool allReady = readyPlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount;

        if (allReady && team1HasPlayer && team2HasPlayer && !gameStarted)
        {
            photonView.RPC("RPC_StartGame", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_StartGame()
    {
        gameStarted = true;
        HideWaitingPanel();

        if (PhotonNetwork.IsMasterClient)
        {
            SpawnBall();

            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            props[GAME_STARTED_KEY] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    private void SpawnBall()
    {
        Vector3 spawnPos = ballSpawnPoint != null ? ballSpawnPoint.position : Vector3.zero;
        ball = PhotonNetwork.Instantiate(ballPrefab.name, spawnPos, Quaternion.identity);
    }

    private void ShowWaitingPanel()
    {
        if (waitingPanel != null)
        {
            waitingPanel.SetActive(true);
            UpdateWaitingText();
        }
    }

    private void HideWaitingPanel()
    {
        if (waitingPanel != null)
            waitingPanel.SetActive(false);
    }

    private void UpdateWaitingText()
    {
        if (waitingText != null)
        {
            waitingText.text = "Waiting for all players to be ready...";
        }
    }

    public void AddScore(int team)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (team == 1)
            team1Score++;
        else if (team == 2)
            team2Score++;

        photonView.RPC("RPC_UpdateScore", RpcTarget.AllBuffered, team1Score, team2Score);

        if (team1Score >= pointsToWin || team2Score >= pointsToWin)
        {
            photonView.RPC("RPC_EndGame", RpcTarget.AllBuffered, team);
        }
        else
        {
            StartCoroutine(RespawnBallAfterDelay(2f));
        }
    }

    [PunRPC]
    private void RPC_UpdateScore(int score1, int score2)
    {
        team1Score = score1;
        team2Score = score2;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (team1ScoreText != null)
            team1ScoreText.text = team1Score.ToString();

        if (team2ScoreText != null)
            team2ScoreText.text = team2Score.ToString();
    }

    private IEnumerator RespawnBallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.IsMasterClient && ball != null)
        {
            PhotonNetwork.Destroy(ball);
            SpawnBall();
        }
    }

    [PunRPC]
    private void RPC_EndGame(int winningTeam)
    {
        gameStarted = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                gameOverText.text = $"Team {winningTeam} Wins!";
            }
        }

        StartCoroutine(ReturnToLobbyAfterDelay(5f));
    }

    private IEnumerator ReturnToLobbyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(menuSceneName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (gameStarted)
        {
            // No permitir nuevos jugadores durante la partida
            Debug.Log("Game already started, player cannot join");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        readyPlayers.Remove(otherPlayer.ActorNumber);
        UpdateReadyUI();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}