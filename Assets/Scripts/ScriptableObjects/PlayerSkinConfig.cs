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
        public RuntimeAnimatorController runtimeController; // Usar solo RuntimeAnimatorController
    }

    [Header("Available Skins")]
    public SkinData[] availableSkins;

    [Header("Default Settings")]
    public int defaultSkinIndex = 0;

    [Header("Shared Animation")]
    public RuntimeAnimatorController defaultAnimatorController; // Controller común para todos los skins

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

        // Prioridad: controller específico del skin > controller por defecto
        if (skinData.runtimeController != null)
            return skinData.runtimeController;

        return defaultAnimatorController;
    }

    public int GetSkinCount()
    {
        return availableSkins.Length;
    }
}