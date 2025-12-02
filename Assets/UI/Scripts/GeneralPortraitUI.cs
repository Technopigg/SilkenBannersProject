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
    public string animatorLayer = "Base Layer";

    [Header("Model Offset")]
    public Vector3 modelPositionOffset = Vector3.zero;  
    public Vector3 modelRotationEuler = Vector3.zero;   
    public Vector3 modelScale = Vector3.one;            

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
        Hide(); 
    }

    void CreateRenderTexture()
    {
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
            portraitCamera.cullingMask = 1 << portraitLayerMask; 
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

        CreateRenderTexture();
        if (currentInstance != null)
        {
            DestroyCurrentInstance();
        }
        currentInstance = Instantiate(generalPrefab, portraitRoot);
        currentInstance.transform.localPosition = modelPositionOffset;
        currentInstance.transform.localEulerAngles = modelRotationEuler;
        currentInstance.transform.localScale = modelScale;
        SetLayerRecursively(currentInstance, portraitLayerName);
        DisableGameplayScripts(currentInstance);
        DisablePhysics(currentInstance);
        
        if (portraitCamera != null)
        {
            portraitCamera.enabled = true;
        }

        // Attempt to play idle animation
        Animator animator = currentInstance.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
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
    
        var monos = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var m in monos)
        {
            if (m == null) continue;
            if (m is Animator) continue; 
        
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
