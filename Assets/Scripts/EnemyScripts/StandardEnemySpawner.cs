using System.Net.NetworkInformation;
using UnityEngine;

public class StandardEnemySpawner : BaseEnemySpawner
{
    private int currentIndex;
    private float nextSpawnTime;

    public override void StartSpawn()
    {
        if (!IsPatternValid())
        {
            isDone = true;
            return;
        }
        isDone = false;
        currentIndex = 0;
        nextSpawnTime = 0f; // spawn first enemy immediately
        Debug.Log($"[Standard] Started wave {waveInstanceId}. Total enemies: {pattern.spawnSequence.Count}");
    }

    public override void Tick(float deltaTime)
    {
        if (isDone) return;

        // Countdown timer
        nextSpawnTime -= deltaTime;
        if (nextSpawnTime <= 0f)
        {
            SpawnNextEnemy();
        }
    }

    private void SpawnNextEnemy()
    {
        if (currentIndex >= pattern.spawnSequence.Count)
        {
            isDone = true;
            Debug.Log($"[Standard] Wave {waveInstanceId} finished (no more enemies).");
            return;
        }

        EnemySpawnEntry entry = pattern.spawnSequence[currentIndex];
        float x = prng.GetPseudoRandomNumber(pattern.spawnMinX, pattern.spawnMaxX);
        float y = prng.GetPseudoRandomNumber(pattern.spawnMinY, pattern.spawnMaxY);
        SpawnEnemy(entry, x, y);
        Debug.Log($"[Standard] Spawned enemy {currentIndex + 1}/{pattern.spawnSequence.Count}");

        // Set delay for next spawn based on the enemy we just spawned
        float delay = (entry.delayBeforeNext > 0) ? entry.delayBeforeNext : pattern.uniformSpaceing;
        nextSpawnTime = delay;
        currentIndex++;

        if (currentIndex >= pattern.spawnSequence.Count)
        {
            isDone = true;
            Debug.Log($"[Standard] Wave {waveInstanceId} all enemies queued, waiting for destruction.");
        }
    }
}