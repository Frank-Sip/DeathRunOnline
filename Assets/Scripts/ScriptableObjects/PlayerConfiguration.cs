using UnityEngine;

[CreateAssetMenu(fileName = "Player Config", menuName = "Player/Player Configuration")]
public class PlayerConfiguration : ScriptableObject
{
    [Header("Player Identity")]
    public string playerName;
    public Sprite playerAvatar;
    public Material playerSkin;
    
    [Header("Player Stats")]
    public MovementStats movementStats;
    
    [Header("Player Role")]
    public string playerTag = "Player";
    public Color tagColor = Color.white;
    
    [Header("Visual Settings")]
    public GameObject playerPrefab;
    public Color nameTagColor = Color.white;
    
    public void UpdatePlayerName(string newName)
    {
        playerName = newName;
    }
    
    public void UpdatePlayerTag(string newTag, Color newTagColor)
    {
        playerTag = newTag;
        tagColor = newTagColor;
    }
}