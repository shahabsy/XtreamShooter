using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnEntry
{
    public EnemyData enemyData;
    public int weight = 1;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemySpawnEntry> spawnableEnemies;
    [SerializeField] private float spawnRate = 1.0f;
    [SerializeField] private Transform enemyContainer;

    private bool isSpawning = false;
    private Coroutine spawnRoutine;
    private int totalWeight;

    private void Start()
    {
        totalWeight = 0;
        foreach(var entry in spawnableEnemies)
        {
            totalWeight += entry.weight;
        }
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        isSpawning = true;
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }
    }

    private IEnumerator SpawnLoop()
    {
        while(isSpawning)
        {
            yield return new WaitForSeconds(spawnRate);
            SpawnRandomEnemy();
        }
    }

    private void SpawnRandomEnemy()
    {
        if (spawnableEnemies.Count == 0) return;

        int randomValue = Random.Range(0, totalWeight);
        EnemyData selectedData = null;
        foreach(var entry in spawnableEnemies)
        {
            if (randomValue < entry.weight)
            {
                selectedData = entry.enemyData;
                break;
            }
            randomValue -= entry.weight;
        }

        if (selectedData == null) return;

        Vector3 spawnPos = new Vector3(12f, Random.Range(-5f, 5f), 0);
        GameObject enemyObj = new GameObject(selectedData.enemyName);
        enemyObj.transform.position = spawnPos;
        enemyObj.transform.SetParent(enemyContainer != null ? enemyContainer : transform);

        var sr = enemyObj.AddComponent<SpriteRenderer>();
        sr.sprite = selectedData.sprite;
        sr.sortingLayerName = selectedData.sortingLayerName;
        sr.sortingOrder = selectedData.orderInLayer;
        enemyObj.transform.localScale = selectedData.spriteScale;

        var enemy = enemyObj.AddComponent<Enemy>();
        enemy.Initialize(selectedData);

        var rb = enemyObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.simulated = false;
        var collider = enemyObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        
        enemyObj.tag = "Enemy";
    }
}
