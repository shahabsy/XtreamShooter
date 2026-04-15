using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveDatabase_", menuName = "Game/WaveDatabase")]
public class WaveDatabase : ScriptableObject
{
    public List<WaveDefinition> waves;

    public string GetRandomWave()
    {
        if (waves == null || waves.Count == 0) return string.Empty;
        return waves[Random.Range(0, waves.Count)].waveName;
    }
}

[System.Serializable]
public class WaveDefinition
{
    public string waveName;
    public GameObject[] enemyPrefabs;
    public EnemyData enemyData;


    public int count;
    public float spacing;
}
