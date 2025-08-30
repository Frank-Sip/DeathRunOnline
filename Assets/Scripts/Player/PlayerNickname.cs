using TMPro;
using UnityEngine;

public class PlayerNickname : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameLabel;
    
    public void Initialize(string playerName)
    {
        nameLabel.text = playerName;
    }
    
    public void UpdateNameLabelOrientation()
    {
        if (nameLabel == null) return;
        
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