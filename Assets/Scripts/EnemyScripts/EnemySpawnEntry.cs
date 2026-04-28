using System;
using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("1D of teh enemy (must match an entry in GameConfig or similar")]
    public EnemyData enemyData;

    public bool useExactPosition = false;
    [Tooltip("Exact world position for this enemy. if null, a random edge position is used.")]
    public Vector2 exactPosition;
    
    [Tooltip("Delay (in seconds) after spawning this enemy before the next enemy is spawned.")]
    public float delayBeforeNext = 0f;
}
