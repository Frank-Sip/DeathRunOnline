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
    [SerializeField] private Transform[] team1SpawnPoints;
    [SerializeField] private Transform[] team2SpawnPoints;

    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballSpawnPoint;

    [Header("UI References")]
    [SerializeField] private TMP_Text team1ScoreText;
    [SerializeField] private TMP_Text team2ScoreText;
    [SerializeField] private TMP_Text readyCountText;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;

    private Dictionary<int, GameObject> playerPaddles = new Dictionary<int, GameObject>();
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

        // VERIFICAR Y ASIGNAR COLOR ANTES DE SPAWN
        if (ColorManager.Instance != null)
        {
            Color availableColor = ColorManager.Instance.GetAvailableColor();
            ColorManager.Instance.SetPlayerColor(availableColor);
        }

        GameObject paddle = PhotonNetwork.Instantiate(paddlePrefab.name, spawnPoint.position, spawnPoint.rotation);

        var paddleController = paddle.GetComponent<PaddleController>();
        if (paddleController != null)
        {
            paddleController.SetTeam(team);
        }
    }

    private int GetPlayerTeam(int actorNumber)
    {
        return ((actorNumber - 1) % 2) + 1;
    }

    private int GetTeamPlayerIndex(int actorNumber, int team)
    {
        return (actorNumber - 1) / 2;
    }

    private Transform GetSpawnPoint(int team, int teamIndex)
    {
        Transform[] spawnPoints = team == 1 ? team1SpawnPoints : team2SpawnPoints;

        if (teamIndex >= spawnPoints.Length)
            teamIndex = spawnPoints.Length - 1;

        return spawnPoints[teamIndex];
    }

    public void NotifyPlayerReady(int actorNumber)
    {
        photonView.RPC("RPC_PlayerReady", RpcTarget.AllBuffered, actorNumber);
    }

    [PunRPC]
    private void RPC_PlayerReady(int actorNumber)
    {
        PaddleController[] allPaddles = FindObjectsOfType<PaddleController>();

        foreach (var paddle in allPaddles)
        {
            if (paddle.photonView.Owner.ActorNumber == actorNumber)
            {
                paddle.isReady = true;
                break;
            }
        }

        UpdateReadyUI();

        if (PhotonNetwork.IsMasterClient)
        {
            CheckStartGame();
        }
    }

    public void OnPlayerReadyChanged()
    {
        UpdateReadyUI();
        
        if (PhotonNetwork.IsMasterClient)
        {
            CheckStartGame();
        }
    }

    private void UpdateReadyUI()
    {
        if (readyCountText != null)
        {
            int readyCount = GetReadyPlayerCount();
            readyCountText.text = $"Ready: {readyCount}/{PhotonNetwork.CurrentRoom.PlayerCount}";
        }

        UpdateWaitingText();
    }

    private int GetReadyPlayerCount()
    {
        int count = 0;
        
        PaddleController[] allPaddles = FindObjectsOfType<PaddleController>();
        
        foreach (var paddle in allPaddles)
        {
            if (paddle.isReady)
            {
                count++;
            }
        }
        
        return count;
    }

    private void CheckStartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (gameStarted) return;

        int readyCount = GetReadyPlayerCount();

        if (readyCount >= 2)
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
            if (PhotonNetwork.CurrentRoom != null)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                Debug.Log("[GameManager2] Room closed - no more players can join");
            }

            photonView.RPC("RPC_InstantiateBall", RpcTarget.AllBuffered);

            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            props[GAME_STARTED_KEY] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    [PunRPC]
    private void RPC_InstantiateBall()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (ballPrefab == null)
            {
                Debug.LogError("[GameManager2] Ball prefab is null!");
                return;
            }

            Vector3 spawnPos = ballSpawnPoint != null ? ballSpawnPoint.position : Vector3.zero;
            GameObject ball = PhotonNetwork.Instantiate(ballPrefab.name, spawnPos, Quaternion.identity);
            Debug.Log($"[GameManager2] Ball instantiated at position: {spawnPos}, ball: {ball.name}");
        }
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
            int readyCount = GetReadyPlayerCount();
            int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

            if (readyCount >= 2)
            {
                waitingText.text = "Starting game...";
            }
            else
            {
                waitingText.text = $"<size=48>Press SPACE when ready</size>\n\n({readyCount}/{totalPlayers} players ready)\n<size=24>At least 2 players needed</size>";
            }
        }
    }

    public void OnGoalScored(int team)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (team == 1) team1Score++;
        else if (team == 2) team2Score++;

        photonView.RPC("RPC_UpdateScore", RpcTarget.AllBuffered, team1Score, team2Score);

        photonView.RPC("RPC_RepositionBall", RpcTarget.AllBuffered);

        if (team1Score >= pointsToWin || team2Score >= pointsToWin)
        {
            photonView.RPC("RPC_EndGame", RpcTarget.AllBuffered, team);
        }
    }

    [PunRPC]
    private void RPC_RepositionBall()
    {
        BallController ball = FindObjectOfType<BallController>();
        
        if (ball != null)
        {
            Vector3 spawnPos = ballSpawnPoint != null ? ballSpawnPoint.position : Vector3.zero;
            ball.transform.position = spawnPos;
            
            Debug.Log($"[GameManager2] Ball repositioned to: {spawnPos}");
            
            if (ball.photonView.IsMine)
            {
                ball.photonView.RPC("RPC_LaunchBall", RpcTarget.AllBuffered);
            }
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

    [PunRPC]
    private void RPC_EndGame(int winningTeam)
    {
        gameStarted = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                // Determinar si el jugador local ganó o perdió
                int localPlayerTeam = GetPlayerTeam(PhotonNetwork.LocalPlayer.ActorNumber);

                if (localPlayerTeam == winningTeam)
                {
                    gameOverText.text = $"<color=green>¡VICTORIA!</color>\n\nTeam {winningTeam} Wins!\n\n<size=24>Returning to lobby...</size>";
                }
                else
                {
                    gameOverText.text = $"<color=red>DERROTA</color>\n\nTeam {winningTeam} Wins\n\n<size=24>Returning to lobby...</size>";
                }
            }
        }

        StartCoroutine(ReturnToLobbyAfterDelay(5f)); // Cambiado a 5 segundos
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
            Debug.Log("Game already started, player cannot join");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateReadyUI();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
