using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance {  get; private set; }

    [Header("Spawn Settings")]
    public float startWait = 3.5f;
    public float quarterDistance = 500f; // scroll distance per quater (instead of time)
    public WaveDatabase waveDatabase;

    private (float min, float max)[] quarterIntervals = new (float min, float max)[4];
    private int currentQuarter = 1;
    private float accumulatedDistance = 0f;
    private float currentMinInterval = 2f;
    private float currentMaxInterval = 2.5f;

    private bool isSpawning = false;
    private Coroutine spawnRoutine;
    private int activeEnemyCount = 0;

    public int ActiveEnemyCount => activeEnemyCount;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetSpawnIntervals(float q1min, float q1max, float q2min, float q2max,
                                    float q3min, float q3max, float q4min, float q4max)
    {
        quarterIntervals[0] = (q1min,  q1max);
        quarterIntervals[1] = (q2min,  q2max);
        quarterIntervals[2] = (q3min,  q3max);
        quarterIntervals[3] = (q4min,  q4max);
        UpdateIntervalForQuarter();
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        isSpawning = true;
        accumulatedDistance = 0;
        currentQuarter = 1;
        UpdateIntervalForQuarter();
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnRoutine != null) StopCoroutine (spawnRoutine);
    }

    public void AddScrolledDistance(float distance)
    {
        if (!isSpawning) return;
        accumulatedDistance += distance;
        if(accumulatedDistance >= quarterDistance && currentQuarter < 4)
        {
            accumulatedDistance -= quarterDistance;
            currentQuarter++;
            UpdateIntervalForQuarter();
            Debug.Log($"EnemySpawner: advanced to quarter {currentQuarter}");
        }
    }

    private void UpdateIntervalForQuarter()
    {
        var (min, max) = quarterIntervals[currentQuarter - 1];
        currentMinInterval = min;
        currentMaxInterval = max;
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(startWait);
        while (isSpawning)
        {
            float interval = Random.Range(currentMaxInterval, currentMaxInterval);
            yield return new WaitForSeconds(interval);
            SpawnRandomEnemy();
        }
    }

    private void SpawnRandomEnemy()
    {
        if (waveDatabase != null && waveDatabase.waves != null && waveDatabase.waves.Count > 0)
        {
            //var randomWave = waveDatabase.waves[Random.Range(0, waveDatabase.waves.Count)];
            SpawnWave(waveDatabase.GetRandomWave());
        }
        else
        {
            Debug.Log("No wave database, spawning fallback enemy");
        }
    }

    public void SpawnWave(string waveName)
    {
        if (waveDatabase == null) return;
        var wave = waveDatabase.waves.Find(w => w.waveName == waveName);
        if (wave != null)
        {
            SpawnWave(wave);
            Debug.Log($"Spawning wave: {waveName}");
        }
        else
        {
            Debug.Log($"Wave: {waveName} not sound in database.");
        }
    }

    public void SpawnWave(WaveDefinition wave)
    {
        if (wave == null || wave.enemyPrefabs == null || wave.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("Wave has no enemy prefabs.");
            return;
        }
        StartCoroutine(SpawnWaveCoroutine(wave));
    }
    private IEnumerator SpawnWaveCoroutine(WaveDefinition wave)
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
                enemy.Initialize(wave.enemyData);
                enemy.OnDeath += () => UnregisterEnemy();

            RegisterEnemy();

            if (wave.spacing > 0 && i < wave.count - 1)
                yield return new WaitForSeconds(wave.spacing);
        }
    }

    public void SpawnEliteWave(string waveId) { /* similar */ }
    public void RegisterEnemy() => activeEnemyCount++;
    public void UnregisterEnemy() => activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
}
