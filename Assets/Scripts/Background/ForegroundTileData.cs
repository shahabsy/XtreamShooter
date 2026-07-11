using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Layer_", menuName = "Game/ForegroundTileData")]
// New foreground layer that blocks view
// Need to Implement: ForegroundTileData.cs in the game to manage foreground elements that block view and interact with the player.
public class ForegroundTileData : ScriptableObject
{
    public List<Sprite> obstacleSprites;
    public float spawnInterval;
    public float speedMultiplier;
}
// Use case: Asteriod fields, debris, structural elements in the foreground that block view and interact with the player.

