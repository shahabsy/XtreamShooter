using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Layer_", menuName = "Game/BackgroundLayerData")]
public class BackgroundLayerData : ScriptableObject
{
    public float scrollSpeed = 1f;
    public float spawnInterval = 2f;

    public bool continuousSpawning = true;
    public List<BackgroundTileData> possibleTiles;

    [Header("Rendering / Sorting")]
    public string sortingLayerName = "Default";
    public int baseOrderInLayer = 0; //pre-group base
    public int OrderGap = 10; // gap between layers (layerIndex * orderGap)
    public float zOffset = 0f; // small zoffset for parallax only
}
