using UnityEngine;
using UnityEngine.UI;

public class SpiritUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text possessionText;
    public Text healthText;
    public Text stateText;

    [Header("Settings")]
    public float fadeSpeed = 2f;

    private SpiritPlayer spiritPlayer;
    private CanvasGroup canvasGroup;

    void Start()
    {
        // Find the spirit player
        spiritPlayer = FindObjectOfType<SpiritPlayer>();
        
        // Get canvas group for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Hide UI initially
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        // Show/hide possession text based on spirit player state
        if (spiritPlayer != null && possessionText != null)
        {
            bool showPossession = spiritPlayer.IsNearPossessionTarget();
            possessionText.gameObject.SetActive(showPossession);
            
            if (showPossession)
            {
                possessionText.text = "Press E to Possess";
                
                // Make it pulse
                float pulse = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
                Color color = possessionText.color;
                color.a = 0.7f + pulse * 0.3f;
                possessionText.color = color;
            }
        }

        // Update state text
        if (stateText != null && GameStateManager.Instance != null)
        {
            stateText.text = "State: " + GameStateManager.Instance.currentState.ToString();
        }

        // Fade in/out based on game state
        if (canvasGroup != null && GameStateManager.Instance != null)
        {
            bool shouldShow = GameStateManager.Instance.IsInState(GameStateManager.GameState.Spirit);
            float targetAlpha = shouldShow ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        if (stateText != null)
        {
            stateText.text = message;
            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), duration);
        }
    }

    void ClearMessage()
    {
        if (stateText != null)
            stateText.text = "";
    }
}
