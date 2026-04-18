using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveDatabase_", menuName = "Game/WaveDatabase")]
public class WaveDatabase : ScriptableObject
{
    public List<WaveDefinition> normalWaves = new List<WaveDefinition>();
    public List<WaveDefinition> eliteWaves = new List<WaveDefinition>();
    public List<BossDefinition> bosses = new List<BossDefinition>();

    public WaveDefinition GetWave(string waveName)
    {
        return normalWaves.Find(w => w.waveName == waveName);
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
}

[System.Serializable]
public class WaveDefinition
{
    public string waveName;
    public GameObject[] enemyPrefabs;
    public EnemyData enemyData;

    //public EnemyAIBehavior behavior;

    public int count = 5;
    public float spacing = 0.5f;
}

[System.Serializable]
public class BossDefinition
{
    public string bossId;
    public string bossName;
    public GameObject bossPrefab;
    public float health = 500;

    public string[] phasePatterns;
}
