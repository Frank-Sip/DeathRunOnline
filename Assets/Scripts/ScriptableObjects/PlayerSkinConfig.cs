using UnityEngine;

[CreateAssetMenu(fileName = "Player Skin Config", menuName = "Player/Player Skin Config")]
public class PlayerSkinConfig : ScriptableObject
{
    [System.Serializable]
    public class SkinData
    {
        public string skinName;
        public GameObject modelPrefab;
        public Sprite skinIcon;
        public Animator animator;
    }

    [Header("Available Skins")]
    public SkinData[] availableSkins;

    [Header("Default Settings")]
    public int defaultSkinIndex = 0;

    public SkinData GetSkinData(int skinIndex)
    {
        if (skinIndex >= 0 && skinIndex < availableSkins.Length)
        {
            return availableSkins[skinIndex];
        }
        return availableSkins[defaultSkinIndex];
    }

    public GameObject GetSkinModel(int skinIndex)
    {
        var skinData = GetSkinData(skinIndex);
        return skinData?.modelPrefab;
    }

    public int GetSkinCount()
    {
        return availableSkins.Length;
    }
}