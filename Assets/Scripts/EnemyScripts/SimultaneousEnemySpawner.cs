using UnityEngine;

public class SimultaneousEnemySpawner : BaseEnemySpawner
{
    public override void StartSpawn()
    {
        if(!IsPatternValid())
        {
            isDone = true;
            return;
        }
        foreach(EnemySpawnEntry entry in pattern.spawnSequence)
        {
            float x = prng.GetPseudoRandomNumber(pattern.spawnMinX, pattern.spawnMaxX);
            float y = prng.GetPseudoRandomNumber(pattern.spawnMinY, pattern.spawnMaxY);

            SpawnEnemy(entry, x, y);
        }
        isDone = true;
    }
}
