using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
using System.Globalization; 

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance;

    [Header("Available Colors")]
    public Color[] availableColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        new Color(1f, 0.5f, 0f), 
        new Color(0.5f, 0f, 1f)  
    };

    private const string PLAYER_COLOR_KEY = "PaddleColor";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ColorManager] Instance created and marked as DontDestroyOnLoad");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Color GetPlayerColor(int actorNumber)
    {
        Debug.Log($"[ColorManager] Getting color for actor {actorNumber}");

        var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        if (player != null)
        {
            Debug.Log($"[ColorManager] Player found. CustomProperties count: {player.CustomProperties.Count}");

            if (player.CustomProperties.ContainsKey(PLAYER_COLOR_KEY))
            {
                string colorString = player.CustomProperties[PLAYER_COLOR_KEY].ToString();
                Debug.Log($"[ColorManager] Color string found: {colorString}");
                Color color = StringToColor(colorString);
                Debug.Log($"[ColorManager] Returning color: {color}");
                return color;
            }
            else
            {
                Debug.LogWarning($"[ColorManager] No color key found for actor {actorNumber}");
            }
        }
        else
        {
            Debug.LogError($"[ColorManager] Player not found for actor {actorNumber}");
        }

        Debug.Log("[ColorManager] Returning default white color");
        return Color.white;
    }

    public void SetPlayerColor(Color color)
    {
        Debug.Log($"[ColorManager] Setting player color: {color}");
        var props = new Hashtable();
        props[PLAYER_COLOR_KEY] = ColorToString(color);
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log($"[ColorManager] Custom properties set. Stored string: {ColorToString(color)}");
    }

    public Color GetAvailableColor()
    {
        Debug.Log("[ColorManager] Getting available color");

        List<Color> colorsInUse = new List<Color>();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey(PLAYER_COLOR_KEY))
            {
                string colorString = player.CustomProperties[PLAYER_COLOR_KEY].ToString();
                colorsInUse.Add(StringToColor(colorString));
            }
        }

        Debug.Log($"[ColorManager] Colors in use: {colorsInUse.Count}");

        foreach (var color in availableColors)
        {
            if (!colorsInUse.Any(c => ColorsAreEqual(c, color)))
            {
                Debug.Log($"[ColorManager] Available color found: {color}");
                return color;
            }
        }

        Color randomColor = availableColors[Random.Range(0, availableColors.Length)];
        Debug.Log($"[ColorManager] All colors in use, returning random: {randomColor}");
        return randomColor;
    }

    private bool ColorsAreEqual(Color a, Color b)
    {
        return Mathf.Approximately(a.r, b.r) &&
               Mathf.Approximately(a.g, b.g) &&
               Mathf.Approximately(a.b, b.b);
    }

    private string ColorToString(Color color)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:F3}|{1:F3}|{2:F3}",
            color.r, color.g, color.b);
    }

    private Color StringToColor(string colorString)
    {
        string[] parts = colorString.Split('|');

        if (parts.Length != 3)
        {
            parts = colorString.Split(',');
        }

        if (parts.Length == 3)
        {
            try
            {
                float r = float.Parse(parts[0], CultureInfo.InvariantCulture);
                float g = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float b = float.Parse(parts[2], CultureInfo.InvariantCulture);
                return new Color(r, g, b);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ColorManager] Error parsing color '{colorString}': {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"[ColorManager] Invalid color string format: '{colorString}' (parts: {parts.Length})");
        }

        return Color.white;
    }
}