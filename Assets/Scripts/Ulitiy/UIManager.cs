using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameObject[] cutsceneUp;
    public GameObject[] cutsceneDown;

    public float tweenDuration = 1.0f; // seconds
    public float moveAmount = 200f; // pixels

    private bool _isTweening;

    public void OnPlayButtonClicked()
    {
        if (!_isTweening)
            StartCoroutine(TweenCutsceneObjectsAndLoadGame());
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator TweenCutsceneObjectsAndLoadGame()
    {
        _isTweening = true;

        float elapsed = 0f;

        // Store original and target positions for CutsceneUp
        Vector3[] startPosUp = new Vector3[cutsceneUp.Length];
        Vector3[] targetPosUp = new Vector3[cutsceneUp.Length];
        for (int i = 0; i < cutsceneUp.Length; i++)
        {
            startPosUp[i] = cutsceneUp[i].transform.localPosition;
            targetPosUp[i] = startPosUp[i] + new Vector3(0, moveAmount, 0);
        }

        // Store original and target positions for CutsceneDown
        Vector3[] startPosDown = new Vector3[cutsceneDown.Length];
        Vector3[] targetPosDown = new Vector3[cutsceneDown.Length];
        for (int i = 0; i < cutsceneDown.Length; i++)
        {
            startPosDown[i] = cutsceneDown[i].transform.localPosition;
            targetPosDown[i] = startPosDown[i] + new Vector3(0, -moveAmount, 0);
        }

        // Tween animation
        while (elapsed < tweenDuration)
        {
            float t = elapsed / tweenDuration;
            t = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < cutsceneUp.Length; i++)
                cutsceneUp[i].transform.localPosition = Vector3.Lerp(startPosUp[i], targetPosUp[i], t);

            for (int i = 0; i < cutsceneDown.Length; i++)
                cutsceneDown[i].transform.localPosition = Vector3.Lerp(startPosDown[i], targetPosDown[i], t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final positions
        for (int i = 0; i < cutsceneUp.Length; i++)
            cutsceneUp[i].transform.localPosition = targetPosUp[i];

        for (int i = 0; i < cutsceneDown.Length; i++)
            cutsceneDown[i].transform.localPosition = targetPosDown[i];

        yield return new WaitForSeconds(0.5f);

        // Load the "Game" scene
        SceneManager.LoadScene("Game");
    }
}
