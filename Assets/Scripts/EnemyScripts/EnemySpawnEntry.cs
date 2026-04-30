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

    [Header("Formation (ignored if useExactPosition")]
    public SpawnFormation formation = SpawnFormation.Solo;
    public int formationSize = 1;
    public float formationSpacing = 1f;
    public Vector2 formationDirection = Vector2.right;

    [Header("Entry Style")]
    public EntryStyle entryStyle = EntryStyle.Instant;
    public float slideInDuration = 0.5f;
    public Vector2 offscreenOffset = new Vector2(2f, 0f);
    
}

public enum SpawnFormation
{
    Solo,
    LineHorizontal,
    LineVertical,
    LineDiagonal,
    VShape,
    Clustered
}

public enum EntryStyle
{
    Instant,
    SlideIn
}
