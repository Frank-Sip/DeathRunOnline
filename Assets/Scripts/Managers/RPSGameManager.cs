using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public enum RPSOption
{
    None,
    Piedra,
    Papel,
    Tijera
}

public class RPSGameManager : MonoBehaviourPunCallbacks
{
    private Dictionary<int, RPSOption> playerChoices = new Dictionary<int, RPSOption>();

    public void ChooseOption(RPSOption option)
    {
        photonView.RPC("RPC_SendChoiceToMaster", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, (int)option);
    }

    [PunRPC]
    void RPC_SendChoiceToMaster(int actorNumber, int option)
    {
        if (!playerChoices.ContainsKey(actorNumber))
            playerChoices.Add(actorNumber, (RPSOption)option);
        else
            playerChoices[actorNumber] = (RPSOption)option;

        Debug.Log($"Jugador {actorNumber} eligió {(RPSOption)option}");

        if (playerChoices.Count == 2)
        {
            DetermineWinner();
        }
    }

    private void DetermineWinner()
    {
        var keys = new List<int>(playerChoices.Keys);
        int player1Id = keys[0];
        int player2Id = keys[1];

        RPSOption p1Choice = playerChoices[player1Id];
        RPSOption p2Choice = playerChoices[player2Id];

        string result = "";

        if (p1Choice == p2Choice)
        {
            result = $"Empate ({p1Choice}) 🤝";
        }
        else if (
            (p1Choice == RPSOption.Piedra && p2Choice == RPSOption.Tijera) ||
            (p1Choice == RPSOption.Papel && p2Choice == RPSOption.Piedra) ||
            (p1Choice == RPSOption.Tijera && p2Choice == RPSOption.Papel)
        )
        {
            result = $"Jugador {player1Id} gana 🏆 ({p1Choice} vs {p2Choice})";
        }
        else
        {
            result = $"Jugador {player2Id} gana 🏆 ({p2Choice} vs {p1Choice})";
        }

        Debug.Log(result);
        photonView.RPC("RPC_AnnounceResult", RpcTarget.All, p1Choice.ToString(), p2Choice.ToString(), result);

        playerChoices.Clear();
    }

    [PunRPC]
    void RPC_AnnounceResult(string p1, string p2, string result)
    {
        Debug.Log($"[RESULTADO] {result}");
        
    }
}

