using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum RPSOption
{
    None = -1,
    Piedra = 0,
    Papel = 1,
    Tijera = 2,
    Lagarto = 3,
    Spock = 4
}

public class RPSGameManager : MonoBehaviourPunCallbacks
{
    public static RPSGameManager Instance;

    [Header("UI Elements")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text selectedChoiceText;

    [Header("Choice Buttons")]
    [SerializeField] private Button piedraButton;
    [SerializeField] private Button papelButton;
    [SerializeField] private Button tijeraButton;
    [SerializeField] private Button lagartoButton;
    [SerializeField] private Button spockButton;
    [SerializeField] private Button sendButton;

    [Header("Settings")]
    [SerializeField] private float resultDisplayTime = 3f;
    [SerializeField] private float victoryDisplayTime = 4f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private RPSOption selectedChoice = RPSOption.None;
    private bool hasSelectedChoice = false;
    private bool gameStarted = false;

    private Dictionary<int, RPSOption> playerChoices = new Dictionary<int, RPSOption>();
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private int currentRound = 1;
    private const int WINNING_SCORE = 2;

    private int player1ActorNumber = -1;
    private int player2ActorNumber = -1;

    #region Unity Lifecycle

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
    }

    private void Start()
    {
        SetupButtons();
        CheckRoomStatus();
    }

    #endregion

    #region Setup

    private void SetupButtons()
    {
        if (piedraButton != null)
            piedraButton.onClick.AddListener(() => SelectChoice(RPSOption.Piedra));
        if (papelButton != null)
            papelButton.onClick.AddListener(() => SelectChoice(RPSOption.Papel));
        if (tijeraButton != null)
            tijeraButton.onClick.AddListener(() => SelectChoice(RPSOption.Tijera));
        if (lagartoButton != null)
            lagartoButton.onClick.AddListener(() => SelectChoice(RPSOption.Lagarto));
        if (spockButton != null)
            spockButton.onClick.AddListener(() => SelectChoice(RPSOption.Spock));
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendChoice);
            sendButton.interactable = false;
        }
    }

    private void CheckRoomStatus()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            ShowWaitingForPlayers();
        }
        else if (!gameStarted && PhotonNetwork.IsMasterClient)
        {
            InitializeGame();
        }
    }

    private void ShowWaitingForPlayers()
    {
        if (resultText != null)
            resultText.text = "Esperando al otro jugador...";

        EnableChoiceButtons(false);
        if (sendButton != null)
            sendButton.interactable = false;
    }

    private void InitializeGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var players = PhotonNetwork.PlayerList;
        player1ActorNumber = players[0].ActorNumber;
        player2ActorNumber = players[1].ActorNumber;

        playerScores.Clear();
        playerScores[player1ActorNumber] = 0;
        playerScores[player2ActorNumber] = 0;

        currentRound = 1;
        gameStarted = true;

        photonView.RPC("RPC_StartGame", RpcTarget.All, player1ActorNumber, player2ActorNumber);
    }

    [PunRPC]
    private void RPC_StartGame(int p1Actor, int p2Actor)
    {
        player1ActorNumber = p1Actor;
        player2ActorNumber = p2Actor;

        playerScores.Clear();
        playerScores[player1ActorNumber] = 0;
        playerScores[player2ActorNumber] = 0;

        currentRound = 1;
        gameStarted = true;

        if (gamePanel != null) gamePanel.SetActive(true);

        ResetRound();
        UpdateUI();

        if (resultText != null)
            resultText.text = "¡Selecciona tu jugada!";
    }

    #endregion

    #region Player Input

    private void SelectChoice(RPSOption choice)
    {
        selectedChoice = choice;
        hasSelectedChoice = true;

        if (sendButton != null)
            sendButton.interactable = true;

        if (selectedChoiceText != null)
            selectedChoiceText.text = $"Seleccionaste: {GetChoiceName(choice)}";

        HighlightSelectedButton(choice);
    }

    private void HighlightSelectedButton(RPSOption choice)
    {
        Button[] buttons = { piedraButton, papelButton, tijeraButton, lagartoButton, spockButton };

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            var colors = buttons[i].colors;
            colors.normalColor = ((int)choice == i) ? new Color(1f, 0.9f, 0.3f) : Color.white;
            buttons[i].colors = colors;
        }
    }

    private void ResetButtonColors()
    {
        Button[] buttons = { piedraButton, papelButton, tijeraButton, lagartoButton, spockButton };

        foreach (var button in buttons)
        {
            if (button == null) continue;
            var colors = button.colors;
            colors.normalColor = Color.white;
            button.colors = colors;
        }
    }

    private void SendChoice()
    {
        if (!hasSelectedChoice || selectedChoice == RPSOption.None) return;

        photonView.RPC("RPC_SendChoiceToMaster", RpcTarget.MasterClient,
            PhotonNetwork.LocalPlayer.ActorNumber, (int)selectedChoice);

        EnableChoiceButtons(false);
        if (sendButton != null)
            sendButton.interactable = false;

        if (resultText != null)
            resultText.text = "Esperando al oponente...";
    }

    private void EnableChoiceButtons(bool enable)
    {
        if (piedraButton != null) piedraButton.interactable = enable;
        if (papelButton != null) papelButton.interactable = enable;
        if (tijeraButton != null) tijeraButton.interactable = enable;
        if (lagartoButton != null) lagartoButton.interactable = enable;
        if (spockButton != null) spockButton.interactable = enable;
    }

    #endregion

    #region Master Client Logic

    [PunRPC]
    private void RPC_SendChoiceToMaster(int actorNumber, int option)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        RPSOption choice = (RPSOption)option;

        if (!playerChoices.ContainsKey(actorNumber))
            playerChoices.Add(actorNumber, choice);
        else
            playerChoices[actorNumber] = choice;

        Debug.Log($"Master recibió: Jugador {actorNumber} eligió {choice}");

        if (playerChoices.Count == 2)
        {
            DetermineWinner();
        }
    }

    private void DetermineWinner()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        RPSOption p1Choice = playerChoices[player1ActorNumber];
        RPSOption p2Choice = playerChoices[player2ActorNumber];

        string player1Name = GetPlayerName(player1ActorNumber);
        string player2Name = GetPlayerName(player2ActorNumber);

        int winner = CalculateWinner(p1Choice, p2Choice);

        string resultMessage;

        if (winner == 0)
        {
            resultMessage = $"¡EMPATE!\n\n{player1Name}: {GetChoiceName(p1Choice)}\n{player2Name}: {GetChoiceName(p2Choice)}";
        }
        else if (winner == 1)
        {
            playerScores[player1ActorNumber]++;
            resultMessage = $"¡{player1Name} GANA ESTA RONDA! \n\n{GetChoiceName(p1Choice)} vence a {GetChoiceName(p2Choice)}";
        }
        else
        {
            playerScores[player2ActorNumber]++;
            resultMessage = $"¡{player2Name} GANA ESTA RONDA! \n\n{GetChoiceName(p2Choice)} vence a {GetChoiceName(p1Choice)}";
        }

        Debug.Log(resultMessage);

        photonView.RPC("RPC_AnnounceResult", RpcTarget.All,
            resultMessage,
            playerScores[player1ActorNumber],
            playerScores[player2ActorNumber]);

        if (playerScores[player1ActorNumber] >= WINNING_SCORE)
        {
            StartCoroutine(AnnounceGameWinner(player1Name));
        }
        else if (playerScores[player2ActorNumber] >= WINNING_SCORE)
        {
            StartCoroutine(AnnounceGameWinner(player2Name));
        }
        else
        {
            currentRound++;
            StartCoroutine(PrepareNextRound());
        }
    }

    private int CalculateWinner(RPSOption choice1, RPSOption choice2)
    {
        if (choice1 == choice2) return 0;

        switch (choice1)
        {
            case RPSOption.Piedra:
                return (choice2 == RPSOption.Tijera || choice2 == RPSOption.Lagarto) ? 1 : 2;
            case RPSOption.Papel:
                return (choice2 == RPSOption.Piedra || choice2 == RPSOption.Spock) ? 1 : 2;
            case RPSOption.Tijera:
                return (choice2 == RPSOption.Papel || choice2 == RPSOption.Lagarto) ? 1 : 2;
            case RPSOption.Lagarto:
                return (choice2 == RPSOption.Papel || choice2 == RPSOption.Spock) ? 1 : 2;
            case RPSOption.Spock:
                return (choice2 == RPSOption.Piedra || choice2 == RPSOption.Tijera) ? 1 : 2;
        }

        return 0;
    }

    private string GetPlayerName(int actorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        return player != null ? player.NickName : $"Jugador {actorNumber}";
    }

    private string GetChoiceName(RPSOption choice)
    {
        switch (choice)
        {
            case RPSOption.Piedra: return "Piedra";
            case RPSOption.Papel: return "Papel";
            case RPSOption.Tijera: return "Tijera";
            case RPSOption.Lagarto: return "Lagarto";
            case RPSOption.Spock: return "Spock";
            default: return "Ninguno";
        }
    }

    #endregion

    #region RPC Results

    [PunRPC]
    private void RPC_AnnounceResult(string resultMessage, int score1, int score2)
    {
        Debug.Log($"[RPC_AnnounceResult] Llamado! Mensaje: {resultMessage}");

        playerScores[player1ActorNumber] = score1;
        playerScores[player2ActorNumber] = score2;

        if (resultText != null)
        {
            resultText.text = resultMessage;
            Debug.Log($"ResultText actualizado: {resultMessage}");
        }
        else
        {
            Debug.LogError("ResultText es NULL!");
        }

        UpdateScoreDisplay();
    }

    private IEnumerator PrepareNextRound()
    {
        yield return new WaitForSeconds(resultDisplayTime);

        playerChoices.Clear();
        photonView.RPC("RPC_StartNewRound", RpcTarget.All, currentRound);
    }

    [PunRPC]
    private void RPC_StartNewRound(int round)
    {
        currentRound = round;

        ResetRound();
        UpdateUI();

        if (resultText != null)
            resultText.text = "¡Selecciona tu jugada!";
    }

    private void ResetRound()
    {
        selectedChoice = RPSOption.None;
        hasSelectedChoice = false;

        if (selectedChoiceText != null)
            selectedChoiceText.text = "Selecciona tu jugada";

        EnableChoiceButtons(true);
        ResetButtonColors();

        if (sendButton != null)
            sendButton.interactable = false;
    }

    private IEnumerator AnnounceGameWinner(string winnerName)
    {
        yield return new WaitForSeconds(resultDisplayTime);

        photonView.RPC("RPC_ShowVictory", RpcTarget.All, winnerName);
    }

    [PunRPC]
    private void RPC_ShowVictory(string winnerName)
    {
        EnableChoiceButtons(false);
        if (sendButton != null)
            sendButton.interactable = false;

        if (resultText != null)
        {
            resultText.text = $"\n\n¡{winnerName} GANÓ LA PARTIDA!\n\n";
            resultText.fontSize = 48;
        }

        StartCoroutine(ReturnToMenuAfterDelay());
    }

    private IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSeconds(victoryDisplayTime);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(mainMenuSceneName);
        }
    }

    #endregion

    #region UI Updates

    private void UpdateUI()
    {
        if (roundText != null)
        {
            roundText.text = $"Ronda {currentRound} - Primero en llegar a 2";
        }

        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null && playerScores.Count == 2)
        {
            string player1Name = GetPlayerName(player1ActorNumber);
            string player2Name = GetPlayerName(player2ActorNumber);

            scoreText.text = $"{player1Name}: {playerScores[player1ActorNumber]}  |  {player2Name}: {playerScores[player2ActorNumber]}";
        }
    }

    #endregion

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Jugador {newPlayer.NickName} entró a la sala");

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && !gameStarted && PhotonNetwork.IsMasterClient)
        {
            InitializeGame();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Jugador {otherPlayer.NickName} salió de la sala");

        if (gameStarted && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_PlayerDisconnected", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_PlayerDisconnected()
    {
        Debug.Log("Un jugador se desconectó. Volviendo al menú...");

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(mainMenuSceneName);
        }
    }

    #endregion
}