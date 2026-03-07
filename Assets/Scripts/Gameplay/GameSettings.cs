using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSettings
{
    // Time Subsystem settings
    [Header("Time Subsystem Settings")]
    public float timePerGameMinute = 3f;
    public float timeScale = 1f;

    // Fading settings
    [Header("Fading Settings")]
    public float ObjectFadeOutAlpha = 0.3f;
    public float ObjectFadeDuration = 0.5f;
    public float transitionFadeDuration = 0.5f;

    // Item pick up settings
    [Header("Item Pick Up Settings")]
    public float itemFlyingSpeed = 5f;
    public float pickupCD_FromWorld = 0.5f;
    public float pickupCD_FromInventory = 2f;
}
