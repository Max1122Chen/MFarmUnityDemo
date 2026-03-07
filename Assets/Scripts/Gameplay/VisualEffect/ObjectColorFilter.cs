using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> spriteRenderersToFilter;
    [SerializeField] private Color filterColor = Color.red;

    public void ApplyColorFilter()
    {
        foreach (var sr in spriteRenderersToFilter)
        {
            if (sr != null)
            {
                sr.color = filterColor;
            }
        }
    }
}
