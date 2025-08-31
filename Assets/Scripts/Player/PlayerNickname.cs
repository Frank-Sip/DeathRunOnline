using TMPro;
using UnityEngine;

public class PlayerNickname : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameLabel;
    
    [Header("Configuration")]
    [SerializeField] private PlayerLabelConfig labelConfig;
    
    private string currentTag;
    private string playerName;
    private bool isVisible = false;
    
    public void Initialize(string playerName)
    {
        this.playerName = playerName;
        UpdateNameLabel();
        SetVisibility(false);
    }
    
    public void SetVisibility(bool visible)
    {
        isVisible = visible;
        nameLabel.gameObject.SetActive(visible);
    }
    
    public void SetPlayerTag(string newTag)
    {
        currentTag = newTag;
        UpdateNameLabel();
    }
    
    private void UpdateNameLabel()
    {
        var tagConfig = labelConfig.GetTagConfig(currentTag);

        if (tagConfig != null)
        {
            string tagColorHex = ColorUtility.ToHtmlStringRGB(tagConfig.tagColor);
            string nicknameColorHex = ColorUtility.ToHtmlStringRGB(labelConfig.nicknameColor);

            nameLabel.text = $"<color=#{tagColorHex}>{tagConfig.tagName}</color> <color=#{nicknameColorHex}>{playerName}</color>";
        }
        else
        {
            nameLabel.text = $"<color=#FFFFFF>{playerName}</color>";
        }
    }
    
    public void UpdateNameLabelOrientation()
    {
        if (nameLabel == null || !isVisible) return;
        
        Camera activeCamera = GetLocalPlayerCamera();
        if (activeCamera != null)
        {
            Vector3 directionToCamera = activeCamera.transform.position - nameLabel.transform.position;
            nameLabel.transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
    }
    
    private Camera GetLocalPlayerCamera()
    {
        PlayerModel[] allPlayers = FindObjectsOfType<PlayerModel>();
        
        foreach (PlayerModel player in allPlayers)
        {
            if (player.PhotonView.IsMine)
            {
                PlayerView playerView = player.GetComponent<PlayerView>();
                if (playerView != null && playerView.PlayerCamera.enabled)
                {
                    return playerView.PlayerCamera;
                }
            }
        }
        
        return null;
    }
}