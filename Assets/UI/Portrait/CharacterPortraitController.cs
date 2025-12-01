using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CharacterPortraitController : MonoBehaviour
{
    [Header("Assign the Portrait model's Animator (can be in another scene)")]
    public Animator portraitAnimator;

    [Header("Optional: scene name if portrait lives in another additive scene")]
    public string portraitSceneName;

    void Start()
    {
        if (portraitAnimator != null)
        {
            PlayIdle();
            return;
        }
        
        if (!string.IsNullOrEmpty(portraitSceneName))
        {
            var s = SceneManager.GetSceneByName(portraitSceneName);
            if (s.isLoaded)
            {
                FindAnimatorInScene(s);
            }
            else
            {
                var op = SceneManager.LoadSceneAsync(portraitSceneName, LoadSceneMode.Additive);
                op.completed += _ => FindAnimatorInScene(SceneManager.GetSceneByName(portraitSceneName));
            }
        }
    }

    void FindAnimatorInScene(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var a = root.GetComponentInChildren<Animator>();
            if (a != null)
            {
                portraitAnimator = a;
                PlayIdle();
                return;
            }
        }

        Debug.LogWarning("Portrait animator not found in scene: " + scene.name);
    }

    public void PlayAnimation(string name)
    {
        if (portraitAnimator == null)
        {
            Debug.LogWarning("No portrait animator assigned.");
            return;
        }

        portraitAnimator.Play(name);
    }

    public void SetTrigger(string trigger)
    {
        if (portraitAnimator == null) return;
        portraitAnimator.SetTrigger(trigger);
    }

    public void PlayIdle()
    {
        if (portraitAnimator == null) return;
        portraitAnimator.Play("BreathingIdle");
    }
}
