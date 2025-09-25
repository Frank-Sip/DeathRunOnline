using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    private readonly List<PlayerModel> aliveRunners = new List<PlayerModel>();
    private Player killerPlayer;
    private bool matchRunning = false;

    [Header("Optional UI")]
    [SerializeField] private GameObject killerVictoryCanvas;
    [SerializeField] private TMPro.TMP_Text killerVictoryText;

    public void StartMatch()
    {
        if (matchRunning) return;

        killerPlayer = GameTagManager.Instance.GetKillerPlayer();
        RegisterCurrentRunners();
        matchRunning = true;

        CheckRunnerCount();
    }

    private void RegisterCurrentRunners()
    {
        aliveRunners.Clear();

        foreach (var pm in FindObjectsOfType<PlayerModel>())
        {
            if (!pm.isAlive) continue;

            string tag = GameTagManager.Instance.GetPlayerTag(pm.PhotonView.Owner);
            if (tag.ToLower() == "runner")
            {
                aliveRunners.Add(pm);
                pm.OnPlayerDeath += OnRunnerDeath;
            }
        }
    }

    private void OnRunnerDeath(PlayerModel runner)
    {
        runner.OnPlayerDeath -= OnRunnerDeath;
        aliveRunners.Remove(runner);
        CheckRunnerCount();
    }

    private void RemoveRunnerByOwner(Player owner)
    {
        for (int i = aliveRunners.Count - 1; i >= 0; i--)
        {
            if (aliveRunners[i].PhotonView.Owner == owner)
            {
                aliveRunners[i].OnPlayerDeath -= OnRunnerDeath;
                aliveRunners.RemoveAt(i);
            }
        }
    }

    private void CheckRunnerCount()
    {
        if (!matchRunning) return;
        if (aliveRunners.Count == 0)
        {
            DeclareKillerVictory();
        }
    }

    private void DeclareKillerVictory()
    {
        matchRunning = false;
        
        string killerNick = killerPlayer != null ? killerPlayer.NickName : "Killer";
        photonView.RPC(nameof(RPC_KillerVictory), RpcTarget.All, killerNick);
    }

    [PunRPC]
    private void RPC_KillerVictory(string killerNickname)
    {
        Debug.Log($"[GameManager] Killer wins! {killerNickname}");

        if (killerVictoryCanvas != null)
        {
            killerVictoryCanvas.SetActive(true);
            killerVictoryText.text = $"{killerNickname} wins!";
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!matchRunning) return;

        string tag = GameTagManager.Instance.GetPlayerTag(otherPlayer);
        if (tag != null && tag.ToLower() == "runner")
        {
            RemoveRunnerByOwner(otherPlayer);
            Debug.Log($"[GameManager] Runner salió de la sala. Restan: {aliveRunners.Count}");
            CheckRunnerCount();
        }
        else if (killerPlayer != null && otherPlayer == killerPlayer)
        {
            Debug.Log("[GameManager] Killer se desconectó. (Acción adicional opcional)");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!matchRunning) return;

        string tag = GameTagManager.Instance.GetPlayerTag(newPlayer);
        if (tag.ToLower() == "runner")
        {
            foreach (var pm in FindObjectsOfType<PlayerModel>())
            {
                if (pm.PhotonView.Owner == newPlayer && pm.isAlive)
                {
                    if (!aliveRunners.Contains(pm))
                    {
                        aliveRunners.Add(pm);
                        pm.OnPlayerDeath += OnRunnerDeath;
                    }
                    break;
                }
            }
            CheckRunnerCount();
        }
        else if (killerPlayer == null)
        {
            killerPlayer = GameTagManager.Instance.GetKillerPlayer();
        }
    }
}