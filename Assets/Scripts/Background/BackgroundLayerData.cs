using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Layer_", menuName = "Game/BackgroundLayerData")]
public class BackgroundLayerData : ScriptableObject
{
    public float scrollSpeed = 1f;
    public float spawnInterval = 2f;

    public bool continuousSpwning = true;
    public List<BackgroundTileData> possibleTiles;
}
