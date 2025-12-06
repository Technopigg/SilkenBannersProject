using UnityEngine;
using UnityEngine.UI;

public class SquadHealthBar : MonoBehaviour
{
    [Header("References")]
    public Squad squad;                      
    public Slider healthSlider;             
    public CanvasGroup canvasGroup;           
    public Transform followOffset;           

    [Header("Settings")]
    public float showSpeed = 8f;              
    public float hideSpeed = 4f;              
    public float hpLerpSpeed = 6f;            

    Camera cam;
    float targetAlpha = 0f;

    void Start()
    {
        cam = Camera.main;

        if (healthSlider == null)
        {
            Debug.LogWarning("SquadHealthBar: No healthSlider assigned.");
            enabled = false;
            return;
        }

        if (canvasGroup == null)
        {
            Debug.LogWarning("SquadHealthBar: No canvasGroup assigned.");
            enabled = false;
            return;
        }

        if (squad != null)
        {
            healthSlider.maxValue = squad.totalMaxHealth;
            healthSlider.value = squad.totalCurrentHealth;
        }

        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (squad == null) return;
        
        healthSlider.maxValue = squad.totalMaxHealth;
        float targetValue = squad.totalCurrentHealth;
        healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * hpLerpSpeed);
        targetAlpha = squad.isSelected ? 1f : 0f;
        float speed = squad.isSelected ? showSpeed : hideSpeed;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * speed);
        
        if (followOffset != null)
            transform.position = followOffset.position;
        else
            transform.position = squad.GetSquadCenter() + Vector3.up * 2f; 
        if (cam != null)
            transform.LookAt(cam.transform);
    }
}
