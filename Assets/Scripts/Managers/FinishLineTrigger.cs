using UnityEngine;
using Photon.Pun;

public class FinishLineTrigger : MonoBehaviourPunCallbacks
{
    private void Start()
    {
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
        if (GameManager.Instance == null || GameManager.Instance.HasGameEnded())
            return;
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
        Debug.Log($"FinishLineTrigger: {nickname} alcanzó la meta. Notificando al GameManager...");

        photonView.RPC("RPC_NotifyRunnerWin", RpcTarget.All, nickname);
    }

    [PunRPC]
    private void RPC_NotifyRunnerWin(string nickname)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessRunnerVictory(nickname);
        }
        else
        {
            Debug.LogError("FinishLineTrigger: GameManager.Instance es null!");
        }
    }
}