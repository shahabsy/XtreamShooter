using UnityEngine;
using System.Collections.Generic;

public enum ScrollingMode { Constant, EaseInEaseOut, SineWave }

[CreateAssetMenu(fileName = "Layer_", menuName = "Game/BackgroundLayerData")]
public class BackgroundLayerData : ScriptableObject
{
    [Header("Scrolling")]
    public float scrollSpeed = 1f;
    public Vector2 scrollSpeedRange = new Vector2(1f, 2f);
    
    [Header("Movement Pattern")]
    public ScrollingMode scrollingMode = ScrollingMode.Constant;
    public float sineAmplitude = 0f;
    public float sineFrequency = 0.5f;
    public float easeDuration = 3f;

    public bool continuousSpawning = true;
    public List<BackgroundTileData> possibleTiles;

    [Header("Rendering / Sorting")]
    public string sortingLayerName = "Default";
    public int baseOrderInLayer = 0;
    public int OrderGap = 10;
    public float zOffset = 0f;
}
