using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameObject[] CutsceneUp;
    public GameObject[] CutsceneDown;

    public float tweenDuration = 1.0f; // seconds
    public float moveAmount = 200f; // pixels

    private bool isTweening = false;

    public void OnPlayButtonClicked()
    {
        if (!isTweening)
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
        isTweening = true;

        float elapsed = 0f;

        // Store original and target positions for CutsceneUp
        Vector3[] startPosUp = new Vector3[CutsceneUp.Length];
        Vector3[] targetPosUp = new Vector3[CutsceneUp.Length];
        for (int i = 0; i < CutsceneUp.Length; i++)
        {
            startPosUp[i] = CutsceneUp[i].transform.localPosition;
            targetPosUp[i] = startPosUp[i] + new Vector3(0, moveAmount, 0);
        }

        // Store original and target positions for CutsceneDown
        Vector3[] startPosDown = new Vector3[CutsceneDown.Length];
        Vector3[] targetPosDown = new Vector3[CutsceneDown.Length];
        for (int i = 0; i < CutsceneDown.Length; i++)
        {
            startPosDown[i] = CutsceneDown[i].transform.localPosition;
            targetPosDown[i] = startPosDown[i] + new Vector3(0, -moveAmount, 0);
        }

        // Tween animation
        while (elapsed < tweenDuration)
        {
            float t = elapsed / tweenDuration;
            t = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < CutsceneUp.Length; i++)
                CutsceneUp[i].transform.localPosition = Vector3.Lerp(startPosUp[i], targetPosUp[i], t);

            for (int i = 0; i < CutsceneDown.Length; i++)
                CutsceneDown[i].transform.localPosition = Vector3.Lerp(startPosDown[i], targetPosDown[i], t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final positions
        for (int i = 0; i < CutsceneUp.Length; i++)
            CutsceneUp[i].transform.localPosition = targetPosUp[i];

        for (int i = 0; i < CutsceneDown.Length; i++)
            CutsceneDown[i].transform.localPosition = targetPosDown[i];

        yield return new WaitForSeconds(0.5f);

        // Load the "Game" scene
        AudioManager.Instance.FadeInMusic(1);
        SceneManager.LoadScene("Game");
    }
}
