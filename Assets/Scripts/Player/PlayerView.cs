using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class PlayerView : MonoBehaviourPun
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;

    [Header("Skin Settings")]
    [SerializeField] private PlayerSkinConfig skinConfig;
    [SerializeField] private Transform modelParent;

    [Header("Model Children")]
    [SerializeField] private GameObject[] modelChildren; 

    private const string SKIN_KEY = "playerSkin";
    private PhotonView photonView;
    private int currentSkinIndex = 0;
    private GameObject currentActiveModel;
    private Animator skinModel;

    public Animator SkinModel => skinModel;
    public Camera PlayerCamera => playerCamera;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        DeactivateAllModels();

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

        Debug.Log($" Remote player camera disabled for {photonView.Owner.NickName}");
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
    private void DeactivateAllModels()
    {
        foreach (GameObject model in modelChildren)
        {
            if (model != null)
            {
                model.SetActive(false);
            }
        }
        currentActiveModel = null;
        skinModel = null;
    }

    private void ApplySkin(int skinIndex)
    {
        if (skinIndex < 0 || skinIndex >= modelChildren.Length)
        {
            Debug.LogError($"Invalid skin index: {skinIndex}. Using default.");
            skinIndex = skinConfig.defaultSkinIndex;
        }

        if (currentActiveModel != null)
        {
            currentActiveModel.SetActive(false);
            NotifyAnimatorModelChanged();
        }

        currentActiveModel = modelChildren[skinIndex];

        if (currentActiveModel != null)
        {
            currentActiveModel.SetActive(true);
            currentSkinIndex = skinIndex;

            SetupAnimator(skinIndex);
            ReconfigurePhotonAnimatorView();

            Debug.Log($"Activated skin model {skinIndex} for {photonView.Owner.NickName}");
        }
        else
        {
            Debug.LogError($"Model child at index {skinIndex} is null!");
        }
    }

    private void SetupAnimator(int skinIndex)
    {
        skinModel = currentActiveModel.GetComponent<Animator>();

        if (skinModel == null)
        {
            Debug.LogError($"No Animator component found on model {currentActiveModel.name}!");
            return;
        }

        RuntimeAnimatorController controller = skinConfig.GetAnimatorController(skinIndex);
        if (controller != null)
        {
            skinModel.runtimeAnimatorController = controller;
            skinModel.Rebind();

            Debug.Log($"AnimatorController assigned and rebound: {controller.name}");
        }
        else
        {
            Debug.LogError($"No AnimatorController found for skin index {skinIndex}");
        }
    }

    private void ReconfigurePhotonAnimatorView()
    {
        PhotonAnimatorView photonAnimatorView = GetComponent<PhotonAnimatorView>();

        if (photonAnimatorView != null && skinModel != null)
        {
            var animatorField = typeof(PhotonAnimatorView).GetField("m_Animator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (animatorField != null)
            {
                animatorField.SetValue(photonAnimatorView, skinModel);
                Debug.Log($"PhotonAnimatorView animator field updated via reflection for {photonView.Owner.NickName}");
            }

            RefreshAnimatorViewParameters(photonAnimatorView);

            Debug.Log($"PhotonAnimatorView reconfigured for {photonView.Owner.NickName}");
        }
        else
        {
            Debug.LogWarning("PhotonAnimatorView not found or skinModel is null");
        }
    }

    private void RefreshAnimatorViewParameters(PhotonAnimatorView photonAnimatorView)
    {
        if (photonView.IsMine)
        {
            photonAnimatorView.enabled = false;
            StartCoroutine(ReenablePhotonAnimatorView(photonAnimatorView));
            Debug.Log("PhotonAnimatorView scheduled for refresh");
        }
    }

    private System.Collections.IEnumerator ReenablePhotonAnimatorView(PhotonAnimatorView photonAnimatorView)
    {
        yield return new WaitForEndOfFrame();

        if (photonAnimatorView != null)
        {
            photonAnimatorView.enabled = true;
            Debug.Log("PhotonAnimatorView re-enabled");
        }
    }

    private void NotifyAnimatorModelChanged()
    {
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.OnModelChanged();
        }
    }

    public bool IsAnimatorReady()
    {
        return skinModel != null && skinModel.runtimeAnimatorController != null;
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

    [ContextMenu("Debug Animator State")]
    public void DebugAnimatorState()
    {
        Debug.Log($"=== ANIMATOR DEBUG for {photonView.Owner.NickName} ===");
        Debug.Log($"SkinModel null: {skinModel == null}");
        Debug.Log($"RuntimeController null: {(skinModel?.runtimeAnimatorController == null)}");
        Debug.Log($"PhotonView IsMine: {photonView.IsMine}");
        Debug.Log($"Current Active Model: {(currentActiveModel != null ? currentActiveModel.name : "null")}");

        PhotonAnimatorView pav = GetComponent<PhotonAnimatorView>();
        Debug.Log($"PhotonAnimatorView null: {pav == null}");
        if (pav != null)
        {
            Debug.Log($"PhotonAnimatorView enabled: {pav.enabled}");
        }
    }
}