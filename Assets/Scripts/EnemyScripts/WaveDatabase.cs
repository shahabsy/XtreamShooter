using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveDatabase_", menuName = "Game/WaveDatabase")]
public class WaveDatabase : ScriptableObject
{
    public List<WaveDefinition> normalWaves = new List<WaveDefinition>();
    public List<WaveDefinition> eliteWaves = new List<WaveDefinition>();
    public List<BossEntry> bosses = new List<BossEntry>();

    public WaveDefinition GetWave(string waveName)
    {
        return normalWaves.Find(w => w.waveName == waveName);
    }

    public WaveDefinition GetEliteWave(string waveName)
    {
        return eliteWaves.Find(w => w.waveName == waveName);
    }

    public BossEntry GetBossEntry(string bossId)
    {
        return bosses.Find(b => b.bossId == bossId);
    }

    public BossData GetBossData(string bossId)
    {
        BossEntry entry = GetBossEntry(bossId);
        return entry?.bossData;
    }

    public GameObject GetBossPrefab(string bossId)
    {
        BossEntry entry = GetBossEntry(bossId);
        return entry?.bossPrefab;
    }

    public WaveDefinition GetRandomWave()
    {
        if (normalWaves.Count == 0) return null;
        return normalWaves[Random.Range(0, normalWaves.Count)];
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
public class BossEntry
{
    public string bossId;
    public string bossName;
    public GameObject bossPrefab;
    public BossData bossData;
}
