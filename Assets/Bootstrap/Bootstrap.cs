using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string worldMapSceneName = "WorldMap";

    void Awake()
    {
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // ✅ Load WorldMap additively so Bootstrap persists
        SceneManager.LoadScene(worldMapSceneName, LoadSceneMode.Additive);

        // ✅ Wait for scene to finish loading before setting it active
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == worldMapSceneName)
        {
            SceneManager.SetActiveScene(scene);
            Debug.Log("WorldMap set as active scene, Bootstrap persists.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}