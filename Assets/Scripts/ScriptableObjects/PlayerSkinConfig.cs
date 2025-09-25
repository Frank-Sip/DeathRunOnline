using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

[CreateAssetMenu(fileName = "Player Skin Config", menuName = "Player/Player Skin Config")]
public class PlayerSkinConfig : ScriptableObject
{
    [System.Serializable]
    public class SkinData
    {
        public string skinName;
        public GameObject modelPrefab;
        public Sprite skinIcon;
        [Header("Animation")]
        public RuntimeAnimatorController runtimeController;
    }

    [Header("Available Skins")]
    public SkinData[] availableSkins;

    [Header("Default Settings")]
    public int defaultSkinIndex = 0;

    [Header("Shared Animation")]
    public RuntimeAnimatorController defaultAnimatorController;

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

    public RuntimeAnimatorController GetAnimatorController(int skinIndex)
    {
        var skinData = GetSkinData(skinIndex);

        if (skinData.runtimeController != null)
            return skinData.runtimeController;

        return defaultAnimatorController;
    }

    public int GetSkinCount()
    {
        return availableSkins.Length;
    }
}