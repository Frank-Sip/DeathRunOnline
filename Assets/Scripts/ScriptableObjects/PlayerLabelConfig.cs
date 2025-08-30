using UnityEngine;

[CreateAssetMenu(fileName = "Player Label Config", menuName = "Player/Player Label Config")]
public class PlayerLabelConfig : ScriptableObject
{
    [System.Serializable]
    public class TagConfig
    {
        public string tagName;
        public Color tagColor;
    }
    
    [Header("Tag Configurations")]
    public TagConfig[] availableTags = new TagConfig[]
    {
        new TagConfig { tagName = "Runner", tagColor = Color.green },
        new TagConfig { tagName = "Killer", tagColor = Color.red },
        new TagConfig { tagName = "Dead", tagColor = Color.gray }
    };
    
    [Header("Label Settings")]
    public Color nicknameColor = Color.white;
    
    public TagConfig GetTagConfig(string tagName)
    {
        foreach (var tag in availableTags)
        {
            if (tag.tagName == tagName)
                return tag;
        }
        return null;
    }
}