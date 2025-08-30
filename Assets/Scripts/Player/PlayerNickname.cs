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
        
        // Buscar la cámara activa (la del jugador local en cada cliente)
        Camera activeCamera = GetLocalPlayerCamera();
        if (activeCamera != null)
        {
            // Hacer que el label mire hacia la cámara del jugador local
            Vector3 directionToCamera = activeCamera.transform.position - nameLabel.transform.position;
            nameLabel.transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
    }
    
    private Camera GetLocalPlayerCamera()
    {
        // Buscar el PlayerView del jugador local (el que tiene IsMine = true)
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