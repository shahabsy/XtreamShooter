using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public float spawnRate = 1f;
    public GameObject[] enemyPrefab;
    private bool isSpawning = false;
    private Coroutine spawnRoutine;

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
        if (enemyPrefab.Length == 0) return;
        GameObject prefab = enemyPrefab[Random.Range(0, enemyPrefab.Length)];
        //Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, Random.Range(0.2f, 0.8f), 0));
        Vector3 spawnPos = new Vector3(10f, Random.Range(-3f, 3f), 0);
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        enemy.transform.SetParent(transform);
    }
}
