using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class VNEventManager : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 1.5f;
    public float blackScreenHoldTime = 1f;

    [Header("Cameras")]
    public Camera currentCamera;
    public List<Camera> availableCameras; // assign all cameras in the scene

    [Header("Rival Movement")]
    public Transform rivalModel;
    public Transform rivalCaughtPosition;
    public float rivalMoveDuration = 1f;

    void Start()
    {
        StartCoroutine(Fade(0)); // fade back in
    }
    
    // Switch camera by index
    public void FadeOutAndChangeCamera(int cameraID)
    {
        if (cameraID < 0 || cameraID >= availableCameras.Count)
        {
            Debug.LogWarning($"Invalid cameraID {cameraID}");
            return;
        }

        StartCoroutine(FadeOutAndTeleport(availableCameras[cameraID]));
    }

    private IEnumerator FadeOutAndTeleport(Camera targetCamera)
    {
        yield return Fade(1); // fade to black

        if (currentCamera != null)
            currentCamera.transform.parent.gameObject.SetActive(false);

        if (targetCamera != null)
        {
            targetCamera.transform.parent.gameObject.SetActive(true);
            currentCamera = targetCamera;
        }

        yield return new WaitForSeconds(blackScreenHoldTime);

        yield return Fade(0); // fade back in
    }

    private IEnumerator Fade(float targetAlpha)
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

    public void RivalCaught()
    {
        if (rivalModel != null && rivalCaughtPosition != null)
            StartCoroutine(MoveRival());
    }

    private IEnumerator MoveRival()
    {
        Vector3 startPos = rivalModel.position;
        Vector3 endPos = rivalCaughtPosition.position;
        float elapsed = 0f;

        while (elapsed < rivalMoveDuration)
        {
            elapsed += Time.deltaTime;
            rivalModel.position = Vector3.Lerp(startPos, endPos, elapsed / rivalMoveDuration);
            yield return null;
        }

        rivalModel.position = endPos;
    }
    
    public void FadeToBlack()
    {
        StartCoroutine(FadeOutOnly());
    }

    private IEnumerator FadeOutOnly()
    {
        yield return Fade(1); // fade to black
        SceneManager.LoadScene(0);
    }
}
