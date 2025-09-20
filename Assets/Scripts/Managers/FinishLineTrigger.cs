using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class FinishLineTrigger : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private GameObject winnerCanvas;
    [SerializeField] private TMP_Text winnerText;

    [Header("Game Settings")]
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool gameEnded = false;
    private string winnerNickname = "";

    private void Start()
    {
        // Asegurar que el canvas esté desactivado al inicio
        if (winnerCanvas != null)
            winnerCanvas.SetActive(false);

        // Configurar el trigger
        var collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError("FinishLineTrigger: No hay Collider en este GameObject");
        }
        else
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el juego ya terminó
        if (gameEnded) return;

        // Verificar si es un jugador con PhotonView
        PhotonView playerPhotonView = other.GetComponent<PhotonView>();
        if (playerPhotonView == null) return;

        // Solo procesar si es el jugador local (evita duplicados)
        if (!playerPhotonView.IsMine) return;

        // Verificar si el jugador es un Runner
        string playerTag = GameTagManager.Instance.GetPlayerTag(playerPhotonView.Owner);
        if (playerTag == null || playerTag.ToLower() != "runner")
        {
            Debug.Log($"Player {playerPhotonView.Owner.NickName} no es un Runner. Tag: {playerTag}");
            return;
        }

        // El primer runner que llegue gana
        string nickname = playerPhotonView.Owner.NickName;
        Debug.Log($"¡{nickname} ha llegado a la meta!");

        // Llamar RPC para mostrar el ganador a todos los jugadores
        photonView.RPC("RPC_ShowWinner", RpcTarget.All, nickname);
    }

    [PunRPC]
    private void RPC_ShowWinner(string nickname)
    {
        if (gameEnded) return;

        gameEnded = true;
        winnerNickname = nickname;

        // Mostrar el canvas del ganador
        if (winnerCanvas != null)
        {
            winnerCanvas.SetActive(true);

            if (winnerText != null)
            {
                winnerText.text = $"{nickname} Ganó!";
            }
        }

        Debug.Log($"¡{nickname} ha ganado la partida!");

        // Iniciar la corrutina para cargar el menú principal
        StartCoroutine(LoadMainMenuAfterDelay());
    }

    private IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        PhotonNetwork.LoadLevel(mainMenuSceneName);
        
    }

    // Método público para cambiar el tiempo de display desde el inspector o código
    public void SetDisplayTime(float time)
    {
        displayTime = time;
    }

    // Método público para obtener información del ganador
    public string GetWinnerNickname()
    {
        return winnerNickname;
    }

    public bool HasGameEnded()
    {
        return gameEnded;
    }
}