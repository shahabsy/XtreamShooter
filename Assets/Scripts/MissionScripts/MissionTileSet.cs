using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "MissionTileSet", menuName = "Game/MissionTileSet")]
public class MissionTileSet : ScriptableObject
{
    [Header("Background Layer")]
    public BackgroundLayerData[] layers;

    [Header("Environment settings")]
    public float baseScrollSpeed = 1f;

    [Header("Audio")]
    public AudioClip ambientMusic;
    public float ambientMusicVolume = 1f;
    public AudioClip bossMusic;
    public float bossMusicVolume = 1f;

    [Header("Visual Overrides")]
    public Color ambientColor = Color.white;
    
    [Header("Dynamic Lighting")]
    public bool useDynamicLighting = false;
    public Light2D ambientLight;
    public Gradient timeOfDayGradient;
    public float transitionDuration = 5f;
}
