using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DataEnemySpawnPattern_", menuName = "Game/DataEnemySpawnPattern")]
public class DataEnemySpawnPattern : ScriptableObject
{
    [Header("Identity")]
    public string patternName;

    [Header("Spawn Sequence")]
    public List<EnemySpawnEntry> spawnSequence;

    [Header("Spawner Type")]
    public SpawnerType spawnerType = SpawnerType.Standard;

    [Header("Standard Spawner(Random Edge + Uniform Spaceing")]
    public float spawnMinX = 10f;
    public float spawnMaxX = 12f;
    public float spawnMinY = -3f;
    public float spawnMaxY = 3f;
    public float uniformSpaceing = 0.5f; // seconds between each spawn

    [Header("RapidFire Spawner(Random Edge + Random Interval")]
    [Tooltip("Minimum seconds between spawns")]
    public float minSpawnInterval = 0.2f;
    [Tooltip("Maximum seconds between spawns")]
    public float maxSpawnInterval = 0.5f;
}
