using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class PlayerView : MonoBehaviourPun
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;

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

    private Animator skinModel;

    public Animator SkinModel => skinModel;

    public Camera PlayerCamera => playerCamera;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            ConfigureLocalPlayer();
        }
        else
        {
            DisableRemotePlayerCamera();
        }
    }

    private void ConfigureLocalPlayer()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            playerCamera.gameObject.SetActive(true);
        }
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = true;
        }
        if (freeLookCamera != null)
        {
            freeLookCamera.enabled = true;
            freeLookCamera.gameObject.SetActive(true);
            freeLookCamera.Priority = 10;
            freeLookCamera.Follow = this.transform;
            freeLookCamera.LookAt = this.transform;
        }

        Debug.Log($"Local player camera configured for {photonView.Owner.NickName}");
    }

    private void DisableRemotePlayerCamera()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
            playerCamera.gameObject.SetActive(false);
        }
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }
        if (freeLookCamera != null)
        {
            freeLookCamera.enabled = false;
            freeLookCamera.gameObject.SetActive(false);
        }

        Debug.Log($"❌ Remote player camera disabled for {photonView.Owner.NickName}");
    }

    private void Start()
    {
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

    private void OnPlayerPropertiesUpdate(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == 253)
        {
            ApplySkinFromProperties();
        }
    }

    public void ApplySkinFromProperties()
    {
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
        }

        this.skinModel = skinModel.GetComponent<Animator>();
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