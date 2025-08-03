using UnityEngine;
using UnityEngine.UI;

public class SimpleHealthBar : MonoBehaviour
{
    [Header("Health Bar")]
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    
    private Health healthComponent;
    private Camera cam;
    private Slider healthSlider;
    private GameObject healthBarCanvas;
    
    void Start()
    {
        cam = Camera.main;
        healthComponent = GetComponent<Health>();
        
        if (healthComponent == null)
        {
            Debug.LogError("No Health component found on " + gameObject.name);
            enabled = false;
            return;
        }
        
        // Create simple health bar UI
        CreateHealthBar();
    }
    
    void CreateHealthBar()
    {
        // Create canvas
        healthBarCanvas = new GameObject("HealthCanvas");
        healthBarCanvas.transform.SetParent(transform);
        healthBarCanvas.transform.localPosition = offset;
        
        Canvas canvas = healthBarCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        
        // Scale down the canvas much more
        healthBarCanvas.transform.localScale = Vector3.one * 0.005f;
        
        // Create slider GameObject
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(healthBarCanvas.transform, false);
        
        // Add slider component
        healthSlider = sliderGO.AddComponent<Slider>();
        
        // Setup slider rect
        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(60, 8);
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Create background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        
        // Add RectTransform explicitly
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.5f);
        
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create fill area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        
        // Add RectTransform explicitly
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Create fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        
        // Add RectTransform explicitly
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = Color.green;
        
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        // Setup slider
        healthSlider.fillRect = fillRect;
        healthSlider.minValue = 0;
        healthSlider.maxValue = 1;
        healthSlider.value = 1f;
    }
    
    void Update()
    {
        if (healthSlider == null || healthComponent == null || healthBarCanvas == null) return;
        
        // Update position
        healthBarCanvas.transform.position = transform.position + offset;
        
        // Face camera
        if (cam != null)
        {
            healthBarCanvas.transform.LookAt(cam.transform);
            healthBarCanvas.transform.Rotate(0, 180, 0);
        }
        
        // Update health value
        float healthPercent = healthComponent.currentHealth / healthComponent.maxHealth;
        healthSlider.value = healthPercent;
        
        // Change color based on health
        Image fillImage = healthSlider.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(Color.red, Color.green, healthPercent);
        }
        
        // Hide when full health
        healthBarCanvas.SetActive(healthPercent < 1f);
    }
}
