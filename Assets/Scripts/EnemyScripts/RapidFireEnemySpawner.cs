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
        float x = prng.GetPseudoRandomNumber(pattern.spawnMinX, pattern.spawnMaxX);
        float y = prng.GetPseudoRandomNumber(pattern.spawnMinY, pattern.spawnMaxY);

        SpawnEnemy(entry, x, y);

        currentIndex++;
        if(currentIndex >= pattern.spawnSequence.Count)
        {
            isDone = true;
        }
    }
    private float GetNextInterval() => prng.GetPseudoRandomNumber(pattern.minSpawnInterval, pattern.maxSpawnInterval);
}
