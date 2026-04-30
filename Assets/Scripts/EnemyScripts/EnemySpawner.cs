using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public WaveDatabase waveDatabase;

    private Camera cachedCamera;
    private HashSet<string> activeSpawningWaves = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        cachedCamera = Camera.main;
    }

    private string NewWaveInstanceId(string baseName) => $"{baseName}_{DateTime.UtcNow.Ticks}";

    public string SpawnWaveAndReturnId(string waveName, bool isElite = false)
    {
        WaveDefinition wave = isElite
            ? waveDatabase.GetEliteWave(waveName) : waveDatabase.GetWave(waveName);

        if (waveDatabase == null || cachedCamera == null) return string.Empty;

        if (wave == null) return string.Empty;

        string waveId = NewWaveInstanceId(waveName);
        StartCoroutine(SpawnWaveCoroutine(wave, isElite, waveId));
        return waveId;
    }

    public string SpawnEliteWaveAndReturnId(string waveName) => SpawnWaveAndReturnId(waveName, true);


    private IEnumerator SpawnWaveCoroutine(WaveDefinition wave, bool isElite, string waveId)
    {
        activeSpawningWaves.Add(waveId);
        Debug.Log($"spawning wave {waveId}");

        BaseEnemySpawner spawner = CreateSpawner(wave.spawnPattern,
            wave.spawnType, isElite, false, waveId);

        spawner.StartSpawn();

        while (!spawner.IsDone)
        {
            spawner.Tick(Time.deltaTime);
            yield return null;
        }
        activeSpawningWaves.Remove(waveId);
        //Debug.Log($"Finished spawning wave {waveId}");
    }
    private BaseEnemySpawner CreateSpawner(DataEnemySpawnPattern pattern, SpawnerType type, bool isElite, bool isBoss, string waveId)
    {
        PRNG pRNG = new PRNG(UnityEngine.Random.Range(0, int.MaxValue));
        BaseEnemySpawner spawner;

        switch (type)
        {
            case SpawnerType.Positioned:
                spawner = new PositionedEnemySpawner();
                break;
            case SpawnerType.RapidFire:
                spawner = new RapidFireEnemySpawner();
                break;
            case SpawnerType.Simultaneous:
                spawner = new SimultaneousEnemySpawner();
                break;
            default:
                spawner = new StandardEnemySpawner();
                break;
        }
        spawner.Initialize(pattern, pRNG, waveId, isElite, isBoss, cachedCamera);
        return spawner;
    }
    public static GameObject GenericSpawnEnemyAtPositionReturn(EnemyData enemyData, int prngSeed, float x, float y, bool isElite, bool isBoss, int overrideId, string waveInstanceId, DataEnemySpawnPattern sourcePattern)
    {
        if (enemyData == null || enemyData.prefab == null) return null;
        GameObject obj = Instantiate(enemyData.prefab, new Vector2(x, y), Quaternion.identity);
        Enemy enemy = obj.GetComponent<Enemy>();
        if(enemy != null)
        {
            enemy.Initialize(enemyData, isElite, isBoss);
            enemy.sourceSpawnPattern = sourcePattern;
            EntityTracker.Instance?.AssignEnemyToWave(enemy, waveInstanceId);
        }
        return obj;
    }
    public static void GenericSpawnEnemyAtPosition(EnemyData enemyData, int pRNGSeed,
        float x, float y, bool isElite, bool isBoss, int overrideId, string waveInstanceId, DataEnemySpawnPattern sourcePattern)
    {
        if(enemyData == null) return;
        if(enemyData.prefab == null) return;
        GameObject enemyObj = Instantiate(enemyData.prefab, new Vector2(x, y), Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(enemyData, isElite, isBoss);
            enemy.sourceSpawnPattern = sourcePattern;
            EntityTracker.Instance.AssignEnemyToWave(enemy, waveInstanceId);
        }
    }

    public void SpawnBoss(string bossId)
    {
        if (waveDatabase == null || cachedCamera == null) return;

        BossData bossData = waveDatabase.GetBossData(bossId);
        GameObject bossPrefab = waveDatabase.GetBossPrefab(bossId);
        if (bossData == null || bossPrefab == null) return;


        
        Vector3 spawnPos = cachedCamera.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 0));
        spawnPos.z = 0;

        GameObject bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        var boss = bossObj.GetComponent<Boss>();
        boss?.Initialize(bossData);
    }

    public IEnumerator SpawnBossRoutine(string bossId)
    {
        if (waveDatabase == null || cachedCamera == null) yield break;
        BossData bossData = waveDatabase.GetBossData(bossId);
        GameObject bossPrefab = waveDatabase.GetBossPrefab(bossId);
        if (bossData == null || bossPrefab == null) yield break;

        Vector3 spawnPos = cachedCamera.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 0));
        spawnPos.z = 0;

        GameObject bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        Boss boss = bossObj.GetComponent<Boss>();
        if (boss != null) boss.Initialize(bossData);

        yield return null;
    }

    public bool IsWaveSpawning(string waveId) => activeSpawningWaves.Contains(waveId);
    
    public void ResetSpawner()
    {
        StopAllCoroutines();
        activeSpawningWaves.Clear();
        //Debug.Log("EnemySpawner reset.");
    }
}
