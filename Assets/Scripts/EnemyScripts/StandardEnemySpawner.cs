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
            SpawnNextEnemyGroup();
        }
    }

    private void SpawnNextEnemyGroup()
    {
        if (currentIndex >= pattern.spawnSequence.Count)
        {
            isDone = true;
            Debug.Log($"[Standard] Wave {waveInstanceId} finished (no more enemies).");
            return;
        }

        EnemySpawnEntry entry = pattern.spawnSequence[currentIndex];
        Vector2 basePos = GetBasePosition(entry);

        for(int i = 0; i < entry.formationSize; i++)
        {
            Vector2 offset = GetFormationOffset(entry, i);
            Vector2 targetPos = basePos + offset;

            Enemy enemy = SpawnSingleEnemy(entry, targetPos.x, targetPos.y);
            if(enemy != null && entry.entryStyle == EntryStyle.SlideIn)
            {
                Vector2 startPos = targetPos + entry.offscreenOffset;
                enemy.BeginSlideIn(startPos, targetPos, entry.slideInDuration);
            }
        }

        // Set delay for next spawn based on the enemy we just spawned
        float delay = (entry.delayBeforeNext > 0) ? entry.delayBeforeNext : pattern.uniformSpacing;
        nextSpawnTime = delay;
        currentIndex++;

        if (currentIndex >= pattern.spawnSequence.Count)
        {
            isDone = true;
            Debug.Log($"[Standard] Wave {waveInstanceId} all enemies queued, waiting for destruction.");
        }
    }

    private Vector2 GetBasePosition(EnemySpawnEntry entry)
    {
        if (entry.useExactPosition) return entry.exactPosition;

        switch (pattern.spawnPositionType)
        {
            case StandardSpawnPosition.MidScreen:
                return GetViewportPosition(1f, 0.5f);
            case StandardSpawnPosition.UpperMiddle:
                return GetViewportPosition(1f, 0.75f);
            case StandardSpawnPosition.LowerMiddle:
                return GetViewportPosition(1f, 0.25f);
            case StandardSpawnPosition.CustomFixed:
                if (pattern.fixedSpawnPositions != null && pattern.fixedSpawnPositions.Length > 0)
                {
                    int idx = prng.GetPseudoRandomInt(0, pattern.fixedSpawnPositions.Length);
                    return pattern.fixedSpawnPositions[idx];
                }
                break;
        }
        //Default: RandomRect
        float x = prng.GetPseudoRandomNumber(pattern.spawnMinX, pattern.spawnMaxX);
        float y = prng.GetPseudoRandomNumber(pattern.spawnMinY, pattern.spawnMaxY);
        return new Vector2(x, y);
        
    }
    private Enemy SpawnSingleEnemy(EnemySpawnEntry entry, float x, float y)
    {
        int seed = prng.GetPseudoRandomInt(0, int.MaxValue);
        GameObject obj = EnemySpawner.GenericSpawnEnemyAtPositionReturn(
            entry.enemyData, seed, x, y, isElite, isBoss, -1, waveInstanceId, pattern);
        return obj?.GetComponent<Enemy>();
    }
}