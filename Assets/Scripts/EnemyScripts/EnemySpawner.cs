using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance {  get; private set; }

    [Header("Spawn Settings")]
    public WaveDatabase waveDatabase;

    private int activeEnemyCount = 0;
    public int ActiveEnemyCount => activeEnemyCount;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void SpawnWave(string waveName)
    {
        if(waveDatabase == null) return;

        WaveDefinition wave = waveDatabase.GetWave(waveName);
        if (wave != null) 
        {
            StartCoroutine(SpawnWaveCoroutine(wave, false));
        }
        else
        {
            Debug.LogWarning($"Wave {waveName} not found.");
        }
    }

    public void SpawnEliteWave(string waveName)
    {
        if (waveDatabase == null) return;

        WaveDefinition wave = waveDatabase.GetEliteWave(waveName);
        if (wave != null)
        {
            StartCoroutine(SpawnWaveCoroutine(wave, true));
        }
        else
        {
            Debug.LogWarning($"Elite Wave {waveName} not found.");
            if(waveDatabase.eliteWaves != null)
            {
                foreach (var w in waveDatabase.eliteWaves)
                    Debug.Log($" - {w.waveName}");
            }
        }
    }
    private IEnumerator SpawnWaveCoroutine(WaveDefinition wave, bool isElite)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        for (int i = 0; i < wave.count; i++)
        {
            // Randomly pick a prefab from the wave's list
            GameObject prefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];

            // Spawn at right edge of screen
            Vector3 spawnPos = cam.ViewportToWorldPoint(new Vector3(1.1f, Random.Range(0.2f, 0.8f), 0));
            spawnPos.z = 0;
            GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Register enemy with spawner
            var enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Initialize(wave.enemyData);
                RegisterEnemy();
                enemy.OnDeath += () =>
                {
                    UnregisterEnemy();
                };
                
            }

            if (wave.spacing > 0 && i < wave.count - 1)
                yield return new WaitForSeconds(wave.spacing);
        }
    }

    public void SpawnBoss(string bossId)
    {
        if (waveDatabase == null) return;

        BossDefinition bossDef = waveDatabase.GetBoss(bossId);
        if (bossDef == null || bossDef.bossPrefab == null)
        {
            Debug.LogError($"Boss {bossId} not found or missing prefab.");
            return;
        }

        Camera cam = Camera.main;
        Vector3 spawnPos = cam.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 0));
        spawnPos.z = 0;
        GameObject bossObj = Instantiate(bossDef.bossPrefab, spawnPos, Quaternion.identity);
        var bossController = bossObj.GetComponent<BossController>();
        if (bossController != null)
        {
            bossController.OnDefeat += () =>
            {
                UnregisterEnemy();
                Debug.Log("Boss defeated.");
            };
            RegisterEnemy();
            //bossController.Initialize(bossDef); // need to verify the bosscontroller.cs updates
        }
    }
    public void RegisterEnemy()
    {
        activeEnemyCount++;
    }
    public void UnregisterEnemy()
    {
        activeEnemyCount--;
    }

    public void ClearAllEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        activeEnemyCount = 0;
    }
}
