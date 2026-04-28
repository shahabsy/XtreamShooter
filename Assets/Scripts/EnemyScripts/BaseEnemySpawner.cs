using UnityEngine;

public abstract class BaseEnemySpawner
{
    protected bool isDone;
    public bool IsDone => isDone;

    protected DataEnemySpawnPattern pattern;
    protected PRNG prng;
    protected string waveInstanceId;
    protected bool isElite;
    protected bool isBoss;

    public virtual void Initialize(DataEnemySpawnPattern spawnPattern, PRNG random, string waveId, bool elite, bool boss)
    {
        pattern = spawnPattern;
        prng = random;
        waveInstanceId = waveId;
        isElite = elite;
        isBoss = boss;
        isDone = false;
    }
    protected bool IsPatternValid()
    {
        if(pattern == null) return false;

        if (pattern.spawnSequence == null || pattern.spawnSequence.Count == 0)
            return false;
        
        return true;
    }

    public virtual void StartSpawn() { }

    public virtual void Tick(float deltaTime) { }

    protected void SpawnEnemy(EnemySpawnEntry entry, float spawnX, float spawnY)
    {
        if (entry.enemyData == null) return;

        int seed = prng.GetPseudoRandomInt(0, 99999999);
        EnemySpawner.GenericSpawnEnemyAtPosition(
            entry.enemyData,
            seed,
            spawnX, spawnY,
            isElite, isBoss,
            -1,//overrideId = -1 means generate new Id
            waveInstanceId,
            pattern
        );
    }
}
