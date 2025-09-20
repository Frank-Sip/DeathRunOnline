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
        GameObject skinModelPrefab = skinConfig.GetSkinModel(skinIndex);
        if (skinModelPrefab != null)
        {
            if (currentModel != null)
            {
                DestroyImmediate(currentModel);
                skinModel = null;

                // IMPORTANTE: Notificar que el modelo cambió
                NotifyAnimatorModelChanged();
            }

            currentModel = Instantiate(skinModelPrefab, modelParent);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
            currentSkinIndex = skinIndex;

            // Configurar el Animator ANTES de cualquier sincronización
            SetupAnimator(skinIndex);

            // NUEVO: Reconfigurar Photon Animator View
            ReconfigurePhotonAnimatorView();
        }
    }

    private void SetupAnimator(int skinIndex)
    {
        skinModel = currentModel.GetComponent<Animator>();

        if (skinModel == null)
        {
            skinModel = currentModel.AddComponent<Animator>();
            Debug.Log("Animator component added to skin model");
        }

        // Asignar el AnimatorController
        RuntimeAnimatorController controller = skinConfig.GetAnimatorController(skinIndex);
        if (controller != null)
        {
            skinModel.runtimeAnimatorController = controller;

            // CRÍTICO: Asegurarse que el Animator está inicializado
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
        // Buscar el PhotonAnimatorView en este GameObject o en el padre
        PhotonAnimatorView photonAnimatorView = GetComponent<PhotonAnimatorView>();

        if (photonAnimatorView != null && skinModel != null)
        {
            // MÉTODO CORRECTO: Usar reflection para acceder al campo privado del animator
            var animatorField = typeof(PhotonAnimatorView).GetField("m_Animator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (animatorField != null)
            {
                animatorField.SetValue(photonAnimatorView, skinModel);
                Debug.Log($"PhotonAnimatorView animator field updated via reflection for {photonView.Owner.NickName}");
            }

            // Alternativa: Reinicializar el componente completo
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
        // Método más simple y confiable: reinicializar el componente
        if (photonView.IsMine)
        {
            // Desactivar y reactivar para forzar la reinicialización
            photonAnimatorView.enabled = false;

            // Esperar un frame antes de reactivar
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
        // Notificar a otros componentes que el modelo cambió
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

    // Método para debugging
    [ContextMenu("Debug Animator State")]
    public void DebugAnimatorState()
    {
        Debug.Log($"=== ANIMATOR DEBUG for {photonView.Owner.NickName} ===");
        Debug.Log($"SkinModel null: {skinModel == null}");
        Debug.Log($"RuntimeController null: {(skinModel?.runtimeAnimatorController == null)}");
        Debug.Log($"PhotonView IsMine: {photonView.IsMine}");

        PhotonAnimatorView pav = GetComponent<PhotonAnimatorView>();
        Debug.Log($"PhotonAnimatorView null: {pav == null}");
        if (pav != null)
        {
            Debug.Log($"PhotonAnimatorView enabled: {pav.enabled}");
        }
    }
}