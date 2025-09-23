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
        if (winnerCanvas != null)
            winnerCanvas.SetActive(false);

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
        if (gameEnded) return;

        PhotonView playerPhotonView = other.GetComponent<PhotonView>();
        if (playerPhotonView == null) return;

        if (!playerPhotonView.IsMine) return;

        string playerTag = GameTagManager.Instance.GetPlayerTag(playerPhotonView.Owner);
        if (playerTag == null || playerTag.ToLower() != "runner")
        {
            Debug.Log($"Player {playerPhotonView.Owner.NickName} no es un Runner. Tag: {playerTag}");
            return;
        }

        string nickname = playerPhotonView.Owner.NickName;
        Debug.Log($"¡{nickname} ha llegado a la meta!");

        photonView.RPC("RPC_ShowWinner", RpcTarget.All, nickname);
    }

    [PunRPC]
    private void RPC_ShowWinner(string nickname)
    {
        if (gameEnded) return;

        gameEnded = true;
        winnerNickname = nickname;

        if (winnerCanvas != null)
        {
            winnerCanvas.SetActive(true);

            if (winnerText != null)
            {
                winnerText.text = $"{nickname} Ganó!";
            }
        }

        Debug.Log($"¡{nickname} ha ganado la partida!");

        StartCoroutine(LoadMainMenuAfterDelay());
    }

    private IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        PhotonNetwork.LoadLevel(mainMenuSceneName);
        
    }

    public void SetDisplayTime(float time)
    {
        displayTime = time;
    }

    public string GetWinnerNickname()
    {
        return winnerNickname;
    }

    public bool HasGameEnded()
    {
        return gameEnded;
    }
}