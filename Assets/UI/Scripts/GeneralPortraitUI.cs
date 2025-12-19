using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a champion/general prefab in a dedicated UI portrait using a RenderTexture.
/// </summary>
public class GeneralPortraitUI : MonoBehaviour
{
    public static GeneralPortraitUI Instance { get; private set; }

    [Header("UI")]
    public RawImage portraitImage;
    public Vector2 renderTextureSize = new Vector2(512, 512);

    [Header("Portrait Scene")]
    public Transform portraitRoot;
    public Camera portraitCamera;
    public Light portraitLight;

    [Header("Layers")]
    public string portraitLayerName = "Portrait";
    private int portraitLayerMask;

    [Header("Animation")]
    public string idleStateName = "Idle";

    [Header("Model Offset")]
    public Vector3 modelPositionOffset = Vector3.zero;
    public Vector3 modelRotationEuler = Vector3.zero;
    public Vector3 modelScale = Vector3.one;

    private RenderTexture rt;
    private GameObject currentInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        if (!portraitCamera) Debug.LogError("GeneralPortraitUI: portraitCamera not assigned.");
        if (!portraitRoot) Debug.LogError("GeneralPortraitUI: portraitRoot not assigned.");
        if (!portraitImage) Debug.LogError("GeneralPortraitUI: portraitImage not assigned.");

        portraitLayerMask = LayerMask.NameToLayer(portraitLayerName);
        if (portraitLayerMask == -1)
            Debug.LogWarning($"GeneralPortraitUI: layer '{portraitLayerName}' not found. Please create it.");

        CreateRenderTexture();
        Hide();
    }

    private void CreateRenderTexture()
    {
        int w = Mathf.Max(16, (int)renderTextureSize.x);
        int h = Mathf.Max(16, (int)renderTextureSize.y);

        if (rt != null) rt.Release();
        rt = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);
        rt.Create();

        if (portraitCamera != null)
        {
            portraitCamera.targetTexture = rt;
            portraitCamera.cullingMask = 1 << portraitLayerMask;
        }

        if (portraitImage != null)
            portraitImage.texture = rt;
    }

    public void Show(GameObject championPrefab)
    {
        Debug.Log("[PortraitUI] Show() CALLED");

        if (!championPrefab)
        {
            Debug.LogWarning("[PortraitUI] championPrefab is NULL");
            Hide();
            return;
        }

        Debug.Log($"[PortraitUI] Prefab received: {championPrefab.name}");

        CreateRenderTexture();
        DestroyCurrentInstance();

        currentInstance = Instantiate(championPrefab, portraitRoot);

        Debug.Log("[PortraitUI] Prefab instantiated");

        var renderers = currentInstance.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"[PortraitUI] Renderer count on instance: {renderers.Length}");

        currentInstance.transform.localPosition = modelPositionOffset;
        currentInstance.transform.localEulerAngles = modelRotationEuler;
        currentInstance.transform.localScale = modelScale;

        SetLayerRecursively(currentInstance, portraitLayerName);

        // 🔴 TEMPORARILY DISABLED FOR DEBUGGING
        // DisableGameplayScripts(currentInstance);
        // DisablePhysics(currentInstance);

        if (portraitCamera)
        {
            portraitCamera.enabled = true;
            Debug.Log("[PortraitUI] Portrait camera ENABLED");
        }

        if (portraitLight)
        {
            portraitLight.enabled = true;
            Debug.Log("[PortraitUI] Portrait light ENABLED");
        }

        Animator animator = currentInstance.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.Play(idleStateName);
            Debug.Log("[PortraitUI] Animator found and idle played");
        }
        else
        {
            Debug.LogWarning("[PortraitUI] NO Animator found on prefab");
        }

        if (portraitRoot)
        {
            portraitRoot.gameObject.SetActive(true);
            Debug.Log("[PortraitUI] PortraitRoot SET ACTIVE");
        }

        if (portraitImage)
        {
            portraitImage.enabled = true;
            Debug.Log("[PortraitUI] Portrait RawImage ENABLED");
        }
    }

    public void Hide()
    {
        if (portraitImage) portraitImage.enabled = false;
        if (portraitCamera) portraitCamera.enabled = false;
        if (portraitLight) portraitLight.enabled = false;
        if (portraitRoot) portraitRoot.gameObject.SetActive(false);

        DestroyCurrentInstance();
    }

    private void DestroyCurrentInstance()
    {
        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }
    }

    private void SetLayerRecursively(GameObject go, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1) return;

        foreach (var t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    private void DisableGameplayScripts(GameObject go)
    {
        foreach (var m in go.GetComponentsInChildren<MonoBehaviour>(true))
            if (!(m is Animator)) m.enabled = false;
    }

    private void DisablePhysics(GameObject go)
    {
        foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
        foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false;
    }

    private void OnDestroy()
    {
        if (rt != null) rt.Release();
        DestroyCurrentInstance();
    }
}
