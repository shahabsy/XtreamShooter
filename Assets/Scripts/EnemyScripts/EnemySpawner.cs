using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance {  get; private set; }

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

    public void SpawnWave(string waveName)
    {
        if (waveDatabase == null || cachedCamera == null) return;
        Debug.Log($"EnemySpawner: spawning wave '{waveName}'");
        WaveDefinition wave = waveDatabase.GetWave(waveName);
        if (wave != null)
        {
            string waveInstanceId = NewWaveInstanceId(waveName);
            StartCoroutine(SpawnWaveCoroutine(wave, false, waveInstanceId));
        }
    }

    public void SpawnEliteWave(string waveName)
    {
        if (waveDatabase == null || cachedCamera == null) return;
        Debug.Log($"Elite EnemySpawner: spawning ELITE wave '{waveName}'");
        WaveDefinition wave = waveDatabase.GetEliteWave(waveName);
        if (wave != null)
        {
            string waveInstanceId = NewWaveInstanceId(waveName);
            StartCoroutine(SpawnWaveCoroutine(wave, true, waveInstanceId));
        }
    }

    public string SpawnWaveAndReturnId(string waveName)
    {
        if (waveDatabase == null || cachedCamera == null) return string.Empty;

        WaveDefinition wave = waveDatabase.GetWave(waveName);
        if (wave == null) return string.Empty;

        string waveInstanceId = NewWaveInstanceId(waveName);
        StartCoroutine(SpawnWaveCoroutine(wave, false, waveInstanceId));
        return waveInstanceId;
    }

    public string SpawnEliteWaveAndReturnId(string waveName)
    {
        if (waveDatabase == null || cachedCamera == null) return string.Empty;

        WaveDefinition wave = waveDatabase.GetEliteWave(waveName);
        if (wave == null) return string.Empty;

        string waveInstanceId = NewWaveInstanceId(waveName);
        StartCoroutine(SpawnWaveCoroutine(wave, true, waveInstanceId));
        return waveInstanceId;
    }

    private IEnumerator SpawnWaveCoroutine(WaveDefinition wave, bool isElite, string waveInstanceId)
    {
        activeSpawningWaves.Add(waveInstanceId);
        Debug.Log($"Start spawning wave {waveInstanceId}");
        for (int i = 0; i < wave.count; i++)
        {
            GameObject prefab = wave.enemyPrefabs[UnityEngine.Random.Range(0, wave.enemyPrefabs.Length)];
            Vector3 spawnPos = cachedCamera.ViewportToWorldPoint(new Vector3(1.1f, UnityEngine.Random.Range(0.2f, 0.8f), 0));
            spawnPos.z = 0;
            GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Register enemy with spawner / tracker
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Initialize(wave.enemyData);
                EntityTracker.Instance?.AssignEnemyToWave(enemy, waveInstanceId);
            }

            if (wave.spacing > 0 && i < wave.count - 1)
                yield return new WaitForSeconds(wave.spacing);
        }
        activeSpawningWaves.Remove(waveInstanceId);
        Debug.Log($"Finished spawning wave {waveInstanceId}");
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

    public void ClearAllEnemies()
    {
        EntityTracker.Instance?.ClearAllEntities();
    }  
    
    public void ResetSpawner()
    {
        StopAllCoroutines();
        activeSpawningWaves.Clear();
        Debug.Log("EnemySpawner reset.");
    }
}
