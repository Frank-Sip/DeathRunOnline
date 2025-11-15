using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] string leaderboardKey = "leaderboard_key2";
    [SerializeField] int count = 10;
    [SerializeField] TMPro.TextMeshProUGUI tableText;

    public void Refresh()
    {
        if (!LootLockerBootstrap.SessionStarted)
        {
            tableText.text = "Logueando...";
            return;
        }

        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, response =>
        {
            if (!response.success)
            {
                tableText.text = $"Error: {response.errorData?.message}";
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rank  Name            Role     Time");
            sb.AppendLine("------------------------------------------");

            var items = response.items;

            if (items == null || items.Length == 0)
            {
                sb.AppendLine("No hay records todavia");
            }
            else
            {
                foreach (var item in items)
                {
                    string name = string.IsNullOrEmpty(item.player.name) ? "Player" + item.player.id : item.player.name;

                    string role = ExtractRoleFromMetadata(item.metadata);

                    string timeFormatted = FormatTime(item.score);

                    if (name.Length > 14)
                        name = name.Substring(0, 14);

                    sb.AppendLine($"{item.rank,4}  {name,-14}  {role,-7}  {timeFormatted}");
                }
            }

            tableText.text = sb.ToString();
        });
    }

    private string ExtractRoleFromMetadata(string metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return "Unknown";

        try
        {
            if (metadata.Contains("Killer"))
                return "Killer";
            else if (metadata.Contains("Runner"))
                return "Runner";
            else
                return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private string FormatTime(int milliseconds)
    {
        int seconds = milliseconds / 1000;
        int ms = milliseconds % 1000;
        return $"{seconds}s {ms}ms";
    }

    public void OnSubmitScoreTMP(TMPro.TMP_InputField scoreInput)
    {
        if (int.TryParse(scoreInput.text, out var score))
        {
            LeaderboardService.SubmitScore(score, leaderboardKey, _ => Refresh());
        }
    }

    public void OnSetNameTMP(TMPro.TMP_InputField nameInput)
    {
        PlayerNameHelper.SetPlayerName(nameInput.text);
    }

    public void BackToLobby()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideLeaderboard();
        }
        else
        {
            Debug.LogError("UIManager.Instance is null! Cannot return to lobby.");
        }
    }
}