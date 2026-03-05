using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>(); 

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            // Fade out
            Fade(GameInstance.Instance.gameSettings.ObjectFadeOutAlpha);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            // Fade in
            Fade(1f);
        }
    }

    private void Fade(float targetAlpha)
    {
        foreach(SpriteRenderer spriteRenderer in spriteRenderers)
        {
            StartCoroutine(Fade(spriteRenderer, targetAlpha));
        }
    }

    private IEnumerator Fade(SpriteRenderer spriteRenderer, float targetAlpha)
    {
        float currentAlpha = spriteRenderer.color.a;
        
        float fadeSpeed = Mathf.Abs(targetAlpha - currentAlpha) / GameInstance.Instance.gameSettings.transitionFadeDuration;

        while (!Mathf.Approximately(spriteRenderer.color.a, targetAlpha))
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.MoveTowards(spriteRenderer.color.a, targetAlpha, fadeSpeed * Time.deltaTime));
            yield return null;
        }
    }
}
