using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance {  get; private set; }

    [Header("Spawn Settings")]
    public WaveDatabase waveDatabase;
    private Camera cachedCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        cachedCamera = Camera.main;
    }

    public void SpawnWave(string waveName)
    {
        if(waveDatabase == null || cachedCamera == null) return;

        WaveDefinition wave = waveDatabase.GetWave(waveName);
        if (wave != null) 
            StartCoroutine(SpawnWaveCoroutine(wave, false));
    }

    public void SpawnEliteWave(string waveName)
    {
        if (waveDatabase == null || cachedCamera == null) return;

        WaveDefinition wave = waveDatabase.GetEliteWave(waveName);
        if (wave != null)
        {
            StartCoroutine(SpawnWaveCoroutine(wave, true));
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
    private IEnumerator SpawnWaveCoroutine(WaveDefinition wave, bool isElite)
    {
        if (cachedCamera == null) yield break;

        for (int i = 0; i < wave.count; i++)
        {
            // Randomly pick a prefab from the wave's list
            GameObject prefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];

            // Spawn at right edge of screen
            Vector3 spawnPos = cachedCamera.ViewportToWorldPoint(new Vector3(1.1f, Random.Range(0.2f, 0.8f), 0));
            spawnPos.z = 0;
            GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Register enemy with spawner
            var enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
                enemy.Initialize(wave.enemyData);

            if (wave.spacing > 0 && i < wave.count - 1)
                yield return new WaitForSeconds(wave.spacing);
        }
    }

    public void ClearAllEnemies()
    {
        EntityTracker.Instance?.ClearAllEntities();
    }
}
