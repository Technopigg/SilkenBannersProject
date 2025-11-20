using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSceneState : MonoBehaviour {
    void Awake() {
        DontDestroyOnLoad(gameObject);
        Debug.Log("DebugSceneState: Awake fired");   // ← add this for confirmation
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            Debug.Log("F1 pressed! Running debug...");
            DebugActiveCameras();
            DebugScenes();
        }
    }

    void DebugActiveCameras() {
        Debug.Log("----- CAMERA DEBUG -----");
        foreach (Camera c in Camera.allCameras) {
            Debug.Log($"Camera '{c.name}' enabled={c.enabled} mask={c.cullingMask} targetTexture={(c.targetTexture!=null)}");
        }
    }

    void DebugScenes() {
        Debug.Log("----- SCENE DEBUG -----");
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            Scene s = SceneManager.GetSceneAt(i);
            Debug.Log($"Scene[{i}] {s.name} active={s.isLoaded} isActiveScene={SceneManager.GetActiveScene()==s}");
        }
    }
}