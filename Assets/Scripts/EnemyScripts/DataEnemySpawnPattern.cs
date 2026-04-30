using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DataEnemySpawnPattern_", menuName = "Game/DataEnemySpawnPattern")]
public class DataEnemySpawnPattern : ScriptableObject
{
    [Header("Identity")]
    public string patternName;

    [Header("Spawner Type")]
    public SpawnerType spawnerType = SpawnerType.Standard;

    [Header("Spawn Sequence")]
    public List<EnemySpawnEntry> spawnSequence;

    [Header("Standard Spawner(Random Edge + Uniform Spaceing")]
    public StandardSpawnPosition spawnPositionType = StandardSpawnPosition.RandomRect;
    public float spawnMinX = 10f;
    public float spawnMaxX = 12f;
    public float spawnMinY = -3f;
    public float spawnMaxY = 3f;
    public float uniformSpacing = 0.5f; // seconds between each spawn
    public Vector2[] fixedSpawnPositions;

    [Header("RapidFire Spawner(Random Edge + Random Interval")]
    public float minSpawnInterval = 0.2f;
    public float maxSpawnInterval = 0.5f;
    [Header("RapidFire Spawn Positions")]
    public Vector2[] rapidFireSpawnPoints;

    [Header("Simultaneous spawner settings")]
    public SpawnEdge[] allowedEdges;
}
public enum StandardSpawnPosition
{
    RandomRect,      // use spawnMin/Max rectangle (existing behavior)
    MidScreen,
    UpperMiddle,
    LowerMiddle,
    CustomFixed,     // optional: still allow manual positions if needed
}

public enum SpawnEdge { Top, Bottom, Right }
