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
        spiritPlayer = FindObjectOfType<SpiritPlayer>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        UpdatePossessionText();
        UpdateStateText();
        UpdateVisibility();
    }

    void UpdatePossessionText()
    {
        if (spiritPlayer == null || possessionText == null) return;

        bool showPossession = spiritPlayer.IsNearPossessionTarget();
        possessionText.gameObject.SetActive(showPossession);
        
        if (showPossession)
        {
            possessionText.text = "Press E to Possess";
            float pulse = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
            Color color = possessionText.color;
            color.a = 0.7f + pulse * 0.3f;
            possessionText.color = color;
        }
    }

    void UpdateStateText()
    {
        if (stateText != null)
            stateText.text = "State: " + GameManager.instance.GetCurrentState().ToString();
    }

    void UpdateVisibility()
    {
        bool shouldShow = GameManager.instance.GetCurrentState() == GameManager.GameState.Battle;
        float targetAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        if (stateText == null) return;
        
        stateText.text = message;
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), duration);
    }

    void ClearMessage()
    {
        if (stateText != null)
            stateText.text = "";
    }
}
