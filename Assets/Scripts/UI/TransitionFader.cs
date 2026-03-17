using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionFader : MonoBehaviour
{
    CanvasGroup canvasGroup;


    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        GameMapSubsystem.Instance.onNewSceneLoaded += HandleNewSceneLoaded;
        GameMapSubsystem.Instance.onOldSceneStartUnloading += HandleOldSceneStartUnloading;
    }

    void OnDestroy()
    {
        if(GameMapSubsystem.Instance != null)  // Check if the instance is not null before trying to unsubscribe, to avoid potential null reference errors during application quit.
        {
            GameMapSubsystem.Instance.onNewSceneLoaded -= HandleNewSceneLoaded;
            GameMapSubsystem.Instance.onOldSceneStartUnloading -= HandleOldSceneStartUnloading;
        }
    }

    private void HandleNewSceneLoaded(string sceneName)
    {
        StartCoroutine(Fade(0f));
    }

    private void HandleOldSceneStartUnloading(string sceneName)
    {
        StartCoroutine(Fade(1f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float currentAlpha = canvasGroup.alpha;
        
        float fadeSpeed = Mathf.Abs(targetAlpha - currentAlpha) / GameInstance.Instance.gameSettings.transitionFadeDuration;

        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }


}
