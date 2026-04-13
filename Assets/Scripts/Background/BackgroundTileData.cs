using UnityEngine;

[CreateAssetMenu(fileName = "Tile_", menuName = "Game/BackgroundTileData")]
public class BackgroundTileData : ScriptableObject
{
    public Sprite sprite;
    public int weight = 1;
    public GameObject prefab;
}
