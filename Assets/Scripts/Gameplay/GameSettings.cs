using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSettings
{
    // Time Subsystem settings
    public float timePerGameMinute = 3f;
    public float timeScale = 1f;

    // Fading settings
    public float ObjectFadeOutAlpha = 0.3f;
    public float ObjectFadeDuration = 0.5f;
    public float transitionFadeDuration = 0.5f;
}
