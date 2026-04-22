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
    public AudioClip bossMusic;

    [Header("Visual Overrides")]
    public Color ambientLight = Color.white;
}

