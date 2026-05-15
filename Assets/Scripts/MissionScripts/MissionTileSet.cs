using UnityEngine;

[CreateAssetMenu(fileName = "MissionTileSet", menuName = "Game/MissionTileSet")]
public class MissionTileSet : ScriptableObject
{
    [Header("Background Layer")]
    public BackgroundLayerData[] layers; // order from far to near layers

    [Header("Environment settings")]
    public float baseScrollSpeed = 1f;

    [Header("Audio")]
    public AudioClip ambientMusic;
    public float ambientMusicVolume = 1f;
    public AudioClip bossMusic;
    public float bossMusicVolume = 1f;


    [Header("Visual Overrides")]
    public Color ambientLight = Color.white;
}

