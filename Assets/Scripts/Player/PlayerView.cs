using UnityEngine;
using Photon.Pun;

public class PlayerView : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float mousePitchTopLimit = -80f;
    [SerializeField] private float mousePitchLowLimit = 80f;
    
    [Header("Skin Settings")]
    [SerializeField] private PlayerSkinConfig skinConfig;
    [SerializeField] private Transform modelParent;

    private float cameraPitch;
    private const string SKIN_KEY = "playerSkin";
    private PhotonView photonView;
    private int currentSkinIndex = 0;
    private GameObject currentModel;
    
    public Camera PlayerCamera => playerCamera;
    
    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        
        if (PhotonNetwork.InRoom)
        {
            ApplySkinFromProperties();
        }
    }

    private void OnEnable()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnPlayerPropertiesUpdate;
        }
    }

    private void OnDisable()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnPlayerPropertiesUpdate;
        }
    }

    public void InitializeCamera(bool isLocalPlayer)
    {
        playerCamera.enabled = isLocalPlayer;
    }

    private void OnPlayerPropertiesUpdate(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == 253)
        {
            ApplySkinFromProperties();
        }
    }

    public void ApplySkinFromProperties()
    {
        if (skinConfig == null)
        {
            Debug.LogError($"PlayerSkinConfig NOT ASSIGNED on {gameObject.name}! Please assign it in the Inspector.");
            return;
        }

        if (photonView == null)
        {
            Debug.LogError($"PhotonView not found on {gameObject.name}");
            return;
        }

        if (photonView.Owner == null)
        {
            Debug.LogError($"PhotonView.Owner is null on {gameObject.name}");
            return;
        }

        if (photonView.Owner.CustomProperties.TryGetValue(SKIN_KEY, out object skinValue))
        {
            int skinIndex = (int)skinValue;
            Debug.Log($"Applying skin from properties: index {skinIndex}");
            ApplySkin(skinIndex);
        }
        else
        {
            Debug.Log($"No skin property found, using default index: {skinConfig.defaultSkinIndex}");
            ApplySkin(skinConfig.defaultSkinIndex);
        }
    }

    private void ApplySkin(int skinIndex)
    {
        if (skinConfig == null)
        {
            Debug.LogError("skinConfig is null in ApplySkin");
            return;
        }

        if (modelParent == null)
        {
            Debug.LogError("modelParent is null in ApplySkin");
            return;
        }

        GameObject skinModel = skinConfig.GetSkinModel(skinIndex);
        if (skinModel != null)
        {
            if (currentModel != null)
            {
                DestroyImmediate(currentModel);
            }

            currentModel = Instantiate(skinModel, modelParent);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;

            currentSkinIndex = skinIndex;
            Debug.Log($"Successfully applied skin {skinIndex}: {skinModel.name}");
        }
        else
        {
            Debug.LogError($"Skin model is null for index {skinIndex}");
        }
    }

    public Vector3 GetCameraForward()
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }
}