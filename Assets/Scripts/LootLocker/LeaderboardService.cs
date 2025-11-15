using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardService : MonoBehaviour
{
    public static void SubmitScore(int score, string leaderboardKey, System.Action<bool> onDone = null)
    {
        LootLockerSDKManager.SubmitScore("", score, leaderboardKey, response =>
        {
            if (!response.success)
            {
                Debug.LogError("Fallo el score");
                onDone?.Invoke(false);
                return;
            }
            Debug.Log("Se envio el score");
            onDone?.Invoke(true);
        });
    }

    public static void SubmitScoreWithMetadata(int score, string role, string leaderboardKey, System.Action<bool> onDone = null)
    {
        string metadata = $"{{\"role\":\"{role}\"}}";

        LootLockerSDKManager.SubmitScore("", score, leaderboardKey, metadata, response =>
        {
            if (!response.success)
            {
                Debug.LogError($"[LeaderboardService] Fallo el envío del score: {response.errorData?.message}");
                onDone?.Invoke(false);
                return;
            }
            Debug.Log($"[LeaderboardService] Score enviado: {score}ms como {role}");
            onDone?.Invoke(true);
        });
    }
}