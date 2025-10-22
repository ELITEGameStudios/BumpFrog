using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VNEventManager : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public float fadeDuration = 1.5f;
    public float blackScreenHoldTime = 1f;
    
    [Header("Cameras")]
    public Camera currentCamera;
    public Camera targetCamera;
    
    public void FadeOutAndChangeCamera()
    {
        StartCoroutine(FadeOutAndTeleport());
    }

    IEnumerator FadeOutAndTeleport()
    {
        yield return Fade(1); // fade to black

        // Switch cameras
        if (currentCamera != null && targetCamera != null)
        {
            currentCamera.enabled = false;
            targetCamera.enabled = true;
        }
        
        yield return new WaitForSeconds(blackScreenHoldTime); // <-- pause while black

        yield return Fade(0); // fade back in
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadePanel.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = targetAlpha;
    }
}
