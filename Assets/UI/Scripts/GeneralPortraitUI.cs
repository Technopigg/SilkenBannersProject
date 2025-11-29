using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Creates a portrait preview of a general prefab using a dedicated camera rendering to a RenderTexture,
/// and displays it on a RawImage in the UI. The spawned preview instance is isolated (its gameplay scripts
/// are disabled) and animator is driven to a specified idle state.
/// </summary>
public class GeneralPortraitUI : MonoBehaviour
{
    [Header("UI")]
    public RawImage portraitImage;            // assign the RawImage in the UI
    public Vector2 renderTextureSize = new Vector2(512, 512);

    [Header("Portrait Scene")]
    public Transform portraitRoot;            // empty GameObject where preview instances live (world-space, disabled from gameplay)
    public Camera portraitCamera;             // camera that looks at portraitRoot and renders only PortraitLayer
    public Light portraitLight;               // optional dedicated light for the portrait

    [Header("Layers")]
    public string portraitLayerName = "Portrait"; // recommended to create in Project Settings > Tags & Layers
    private int portraitLayerMask;

    [Header("Animation")]
    public string idleStateName = "Idle";     // animation state name to play on the Animator
    public string animatorLayer = "Base Layer"; // optional

    [Header("Model Offset")]
    public Vector3 modelPositionOffset = Vector3.zero;   // tweak to center model
    public Vector3 modelRotationEuler = Vector3.zero;    // rotate model so it faces camera nicely
    public Vector3 modelScale = Vector3.one;             // scale override if needed

    private RenderTexture rt;
    private GameObject currentInstance;

    void Awake()
    {
        if (portraitCamera == null)
            Debug.LogError("GeneralPortraitUI: portraitCamera not assigned.");

        if (portraitRoot == null)
            Debug.LogError("GeneralPortraitUI: portraitRoot not assigned.");

        if (portraitImage == null)
            Debug.LogError("GeneralPortraitUI: portraitImage (RawImage) not assigned.");

        portraitLayerMask = LayerMask.NameToLayer(portraitLayerName);
        if (portraitLayerMask == -1)
            Debug.LogWarning($"GeneralPortraitUI: layer '{portraitLayerName}' not found. Please create it in Tags & Layers.");

        CreateRenderTexture();
        Hide(); // start hidden
    }

    void CreateRenderTexture()
    {
        // create RT to match requested size (or reuse)
        int w = Mathf.Max(16, (int)renderTextureSize.x);
        int h = Mathf.Max(16, (int)renderTextureSize.y);

        if (rt == null || rt.width != w || rt.height != h)
        {
            if (rt != null) rt.Release();
            rt = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);
            rt.Create();
        }

        if (portraitCamera != null)
        {
            portraitCamera.targetTexture = rt;
            portraitCamera.cullingMask = 1 << portraitLayerMask; // only render the portrait layer
        }
        portraitImage.texture = rt;
    }

    /// <summary>
    /// Show the portrait for a general prefab. This instantiates a preview clone,
    /// strips gameplay scripts, places it in portraitRoot and plays idle animation.
    /// </summary>
    public void Show(GameObject generalPrefab)
    {
        if (generalPrefab == null)
        {
            Debug.LogWarning("GeneralPortraitUI.Show called with null prefab.");
            Hide();
            return;
        }

        // Create RT if necessary (handles resizing while editor runs)
        CreateRenderTexture();

        // If same prefab already displayed, keep it
        if (currentInstance != null)
        {
            // optional: check name, reuse if same prefab type
            // Destroy it and reinstantiate for safety
            DestroyCurrentInstance();
        }

        // instantiate into portraitRoot
        currentInstance = Instantiate(generalPrefab, portraitRoot);
        currentInstance.transform.localPosition = modelPositionOffset;
        currentInstance.transform.localEulerAngles = modelRotationEuler;
        currentInstance.transform.localScale = modelScale;

        // set the layer of instance and all children to portrait layer
        SetLayerRecursively(currentInstance, portraitLayerName);

        // disable gameplay scripts (leave Animator enabled)
        DisableGameplayScripts(currentInstance);

        // if the prefab has Rigidbody / Colliders etc, disable them for preview
        DisablePhysics(currentInstance);

        // Ensure camera looks at the model - user may position camera manually in inspector
        // If portraitCamera isn't set to look at portraitRoot via inspector, we can optionally set it:
        if (portraitCamera != null)
        {
            // you can optionally adjust camera transform here - leaving it to inspector gives better control
            portraitCamera.enabled = true;
        }

        // Attempt to play idle animation
        Animator animator = currentInstance.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // Allow animator to update in unscaled time to keep UI responsive if game is paused
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            // Try Play, wrapped in try/catch to avoid errors if state missing.
            try
            {
                if (!string.IsNullOrEmpty(idleStateName))
                    animator.Play(idleStateName);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("GeneralPortraitUI: Could not Play idle state on Animator: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("GeneralPortraitUI: no Animator found on general prefab instance.");
        }

        // Make portrait visible
        portraitImage.enabled = true;
        if (portraitRoot != null) portraitRoot.gameObject.SetActive(true);
    }

    public void Hide()
    {
        portraitImage.enabled = false;
        DestroyCurrentInstance();

        if (portraitRoot != null)
            portraitRoot.gameObject.SetActive(false);

        if (portraitCamera != null)
            portraitCamera.enabled = false;
    }

    private void DestroyCurrentInstance()
    {
        if (currentInstance != null)
        {
            // Destroy immediately if in editor or playmode
            Destroy(currentInstance);
            currentInstance = null;
        }
    }

    private void SetLayerRecursively(GameObject go, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogWarning($"GeneralPortraitUI: layer '{layerName}' not found.");
            return;
        }

        var transforms = go.GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
            t.gameObject.layer = layer;
    }

    private void DisableGameplayScripts(GameObject go)
    {
        // Disable MonoBehaviours except Animator
        var monos = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var m in monos)
        {
            if (m == null) continue;
            if (m is Animator) continue; // keep animator enabled
            // optionally keep other components: Renderer, Transform, etc.
            m.enabled = false;
        }
    }

    private void DisablePhysics(GameObject go)
    {
        var rbs = go.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
            rb.isKinematic = true;

        var cols = go.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
            c.enabled = false;
    }

    void OnDestroy()
    {
        if (rt != null)
        {
            rt.Release();
            rt = null;
        }
        DestroyCurrentInstance();
    }
}
