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
    protected Camera cachedCamera;

    public virtual void Initialize(DataEnemySpawnPattern spawnPattern, PRNG random, string waveId, bool elite, bool boss, Camera camera)
    {
        pattern = spawnPattern;
        prng = random;
        waveInstanceId = waveId;
        isElite = elite;
        isBoss = boss;
        isDone = false;
        cachedCamera = camera;
    }
    protected bool IsPatternValid()
    {
        if(pattern == null) return false;

        if (pattern.spawnSequence == null || pattern.spawnSequence.Count == 0)
            return false;
        
        return true;
    }
    protected Vector2 GetFormationOffset(EnemySpawnEntry entry, int index)
    {
        float halfSize = (entry.formationSize - 1) / 2f;
        float spacing = entry.formationSpacing;

        switch (entry.formation)
        {
            case SpawnFormation.LineHorizontal:
                return new Vector2((index - halfSize) * spacing, 0);
            case SpawnFormation.LineVertical:
                return new Vector2(0, (index - halfSize) * spacing);
            case SpawnFormation.LineDiagonal:
                float diagonal = (index - halfSize) * spacing;
                return new Vector2(diagonal, diagonal);
            case SpawnFormation.VShape:
                if (index == 0) return Vector2.zero;
                int armIndex = (index +1) / 2;
                int side = (index % 2 == 0) ? 1 : -1;
                Vector2 vOffset = new Vector2(side * armIndex * spacing, armIndex * spacing);
                return new Vector2(vOffset.y, -vOffset.x);
            case SpawnFormation.Clustered:
                float angle = prng.GetPseudoRandomNumber(0, Mathf.PI * 2f);
                float radius = prng.GetPseudoRandomNumber(0, spacing);
                return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            default:
                return Vector2.zero;
        }
    }
    protected Vector2 GetViewportPosition(float vx, float vy)
    {

        if(cachedCamera == null)
        {
            cachedCamera = Camera.main;
            if (cachedCamera == null) return Vector2.zero;
        }
        Vector3 vp = new Vector3(vx, vy, Mathf.Abs(cachedCamera.transform.position.z));
        Vector3 world = cachedCamera.ViewportToWorldPoint(vp);
        return world;
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
