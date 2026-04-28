using UnityEngine;

public class PositionedEnemySpawner : BaseEnemySpawner
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
            if(entry.useExactPosition)
            {
                SpawnEnemy(entry, entry.exactPosition.x, entry.exactPosition.y);
            }
            else
            {
                float x = prng.GetPseudoRandomNumber(pattern.spawnMinX, pattern.spawnMaxX);
                float y = prng.GetPseudoRandomNumber(pattern.spawnMinY, pattern.spawnMaxY);

                SpawnEnemy(entry, x, y);
            }
        }
        isDone = true;
    }
}
