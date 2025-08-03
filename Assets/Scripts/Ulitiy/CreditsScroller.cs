using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CreditsScroller : MonoBehaviour
{
    [Header("Credits Settings")]
    public TextMeshProUGUI creditsText;
    public float scrollSpeed = 50f;
    public float startDelay = 1f;
    public string menuSceneName = "MainMenu";
    
    [Header("Credits Content")]
    [TextArea(10, 20)]
    public string creditsContent = 
        "DEEPBOUND\n\n" +
        "A Game Made for GMTK Game Jam 2025\n\n\n" +
        "PROGRAMMING\n" +
        "Hakashi Katake - Programmer\n" +
        "DnzDev - Programmer\n\n" +
        "ART & DESIGN\n" +
        "Tesstrie - Artist\n" +
        "Stancat - Artist\n" +
        "AUDIO\n" +
        "Qad - Music Composer\n\n\n" +
        "SPECIAL THANKS\n" +
        "Unity Technologies\n" +
        "GMTK Game Jam Community\n" +
        "Unity 2022.3\n" +
        "Visual Studio Code\n" +
        "Thank you for playing!\n\n" +
        "Press ESC or SPACE to return to menu\n\n\n\n\n";
    
    private RectTransform textRect;
    private Canvas canvas;
    private bool isScrolling = false;
    private float startY;
    private float endY;
    
    void Start()
    {
        SetupCredits();
        StartCoroutine(StartScrolling());
    }
    
    void Update()
    {
        HandleInput();
        
        if (isScrolling)
        {
            ScrollText();
        }
    }
    
    void SetupCredits()
    {
        // Get or create canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("CreditsCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create or setup credits text
        if (creditsText == null)
        {
            GameObject textGO = new GameObject("CreditsText");
            textGO.transform.SetParent(canvas.transform, false);
            creditsText = textGO.AddComponent<TextMeshProUGUI>();
            
            // Setup text properties
            creditsText.fontSize = 24;
            creditsText.color = Color.white;
            creditsText.alignment = TextAlignmentOptions.Center;
            creditsText.lineSpacing = 1.2f;
        }
        
        // Set credits content
        creditsText.text = creditsContent;
        
        // Get text rect transform
        textRect = creditsText.GetComponent<RectTransform>();
        
        // Setup text positioning
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Calculate text size
        creditsText.rectTransform.sizeDelta = new Vector2(800, creditsText.preferredHeight);
        
        // Set start and end positions
        startY = -Screen.height - creditsText.preferredHeight / 2;
        endY = Screen.height + creditsText.preferredHeight / 2;
        
        // Position text at bottom of screen
        textRect.anchoredPosition = new Vector2(0, startY);
    }
    
    IEnumerator StartScrolling()
    {
        yield return new WaitForSeconds(startDelay);
        isScrolling = true;
    }
    
    void ScrollText()
    {
        // Move text upward
        Vector2 currentPos = textRect.anchoredPosition;
        currentPos.y += scrollSpeed * Time.deltaTime;
        textRect.anchoredPosition = currentPos;
        
        // Check if credits finished scrolling
        if (currentPos.y >= endY)
        {
            // Credits finished, wait a moment then return to menu
            StartCoroutine(ReturnToMenuAfterDelay(2f));
            isScrolling = false;
        }
    }
    
    void HandleInput()
    {
        // Return to menu on ESC or Space
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            ReturnToMenu();
        }
        
        // Allow mouse click to return to menu
        if (Input.GetMouseButtonDown(0))
        {
            ReturnToMenu();
        }
    }
    
    IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToMenu();
    }
    
    public void ReturnToMenu()
    {
        // Load menu scene
        SceneManager.LoadScene(menuSceneName);
    }
    
    // Public method to set custom credits content
    public void SetCreditsContent(string newContent)
    {
        creditsContent = newContent;
        if (creditsText != null)
        {
            creditsText.text = creditsContent;
        }
    }
    
    // Public method to adjust scroll speed
    public void SetScrollSpeed(float newSpeed)
    {
        scrollSpeed = newSpeed;
    }
}
