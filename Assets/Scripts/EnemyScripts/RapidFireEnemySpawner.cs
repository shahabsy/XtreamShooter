using UnityEngine;

public class RapidFireEnemySpawner : BaseEnemySpawner
{
    private int currentIndex;
    private float timer;
    private float nextInterval;

    public override void StartSpawn()
    {
        if(!IsPatternValid())
        {
            isDone = true;
            return;
        }
        isDone = false;
        currentIndex = 0;
        timer = 0;
        nextInterval = GetNextInterval();

        SpawnCurrentEnemy();
    }
    public override void Tick(float deltaTime)
    {
        if (IsDone) return;
        timer += deltaTime;
        if(timer >= nextInterval)
        {
            timer = 0;
            SpawnCurrentEnemy();
            if (!isDone)
                nextInterval = GetNextInterval();
        }
    }
    private void SpawnCurrentEnemy()
    {
        if(currentIndex >= pattern.spawnSequence.Count)
        {
            isDone = true;
            return;
        }
        EnemySpawnEntry entry = pattern.spawnSequence[currentIndex];
        Vector2 spawnPos = GetSpawnPosition();

        SpawnEnemy(entry, spawnPos.x, spawnPos.y);
        currentIndex++;
        if(currentIndex >= pattern.spawnSequence.Count)
        {
            isDone = true;
        }
    }
    private Vector2 GetSpawnPosition()
    {
        if(pattern.rapidFireSpawnPoints != null && pattern.rapidFireSpawnPoints.Length > 0)
        {
            int idx = prng.GetPseudoRandomInt(0, pattern.rapidFireSpawnPoints.Length);
            return pattern.rapidFireSpawnPoints[idx];
        }
        //Fallback
        float x = prng.GetPseudoRandomNumber(pattern.spawnMinX, pattern.spawnMaxX);
        float y = prng.GetPseudoRandomNumber(pattern.spawnMinY, pattern.spawnMaxY);
        return new Vector2(x, y);
    }
    private float GetNextInterval() => prng.GetPseudoRandomNumber(pattern.minSpawnInterval, pattern.maxSpawnInterval);
}
