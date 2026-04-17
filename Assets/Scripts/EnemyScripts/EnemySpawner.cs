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

    private List<string> normalWaveNames = new List<string>();
    private List<string> eliteWaveNames = new List<string>();

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

    public void SetWaveLists(string[] normalWaves, string[] eliteWaves)
    {
        normalWaveNames.Clear();
        eliteWaveNames.Clear();
        if(normalWaves != null) normalWaveNames.AddRange(normalWaves);
        if (eliteWaves != null) eliteWaveNames.AddRange(eliteWaves);
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
        if(accumulatedDistance >= quarterDistance && currentQuarter < 4) // quarterIntervals.Length
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
            float interval = Random.Range(currentMinInterval, currentMaxInterval);
            yield return new WaitForSeconds(interval);

            if (eliteWaveNames.Count > 0 && Random.value < 0.2)
            {
                SpawnRandomEliteWave();
            }
            else
            {
                SpawnRandomWave();
            }
                
        }
    }

    private void SpawnRandomWave()
    {
        if (normalWaveNames.Count == 0) return;
        string waveName = normalWaveNames[Random.Range(0, normalWaveNames.Count)];
        SpawnWave(waveName);
    }
    private void SpawnRandomEliteWave()
    {
        if (eliteWaveNames.Count == 0) return;
        string waveName = eliteWaveNames[Random.Range(0, eliteWaveNames.Count)];
        //Debug.Log($"Attempting to spawn elite wave: {waveName}");
        SpawnEliteWave(waveName);
    }

    public void SpawnWave(string waveName)
    {
        if(waveDatabase == null) return;
        var wave = waveDatabase.GetWave(waveName);
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
        var wave = waveDatabase.GetEliteWave(waveName);
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
        var bossDef = waveDatabase.GetBoss(bossId);
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
            bossController.Initialize(bossDef);
            
        }
    }
    public void RegisterEnemy() => activeEnemyCount++;
    public void UnregisterEnemy() => activeEnemyCount--;
}
