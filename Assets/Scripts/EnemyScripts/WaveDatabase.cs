using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveDatabase_", menuName = "Game/WaveDatabase")]
public class WaveDatabase : ScriptableObject
{
    public List<WaveDefinition> waves;
    public List<WaveDefinition> eliteWaves;
    public List<BossDefinition> bosses;

    public WaveDefinition GetWave(string waveName)
    {
        return waves.Find(w => w.waveName == waveName);
    }

    public WaveDefinition GetEliteWave(string waveName)
    {
        return eliteWaves.Find(w => w.waveName == waveName);
    }

    public BossDefinition GetBoss(string bossId)
    {
        return bosses.Find(b => b.bossId == bossId);
    }

    public WaveDefinition GetRandomEliteWave()
    {
        if (eliteWaves.Count == 0) return null;
        return eliteWaves[Random.Range(0, eliteWaves.Count)];
    }

    public string GetRandomWave()
    {
        if (waves.Count == 0) return null;
        return waves[Random.Range(0, waves.Count)].waveName;
    }
}

[System.Serializable]
public class WaveDefinition
{
    public string waveName;
    public GameObject[] enemyPrefabs;
    public EnemyData enemyData;
    //public EnemyAIBehavior behavior;

    public int count = 3;
    public float spacing = 0.5f;
}

[System.Serializable]
public class BossDefinition
{
    public string bossId;
    public GameObject bossPrefab;
    public float health = 500;
    public AudioClip introMusic;
}
