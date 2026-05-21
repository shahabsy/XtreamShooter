using UnityEngine;

public abstract class BossSummonBehavior : ScriptableObject
{
    public float summonInterval = 5f;
    public int spawnPointIndex = 0;

    protected Boss boss;
    protected Transform spawnPoint;
    protected float timer;


    public virtual void Initialize(Boss owner, Transform point)
    {
        boss = owner;
        spawnPoint = point;
        timer = 0f;
    }

    public abstract void TrySummon(float deltaTime);

}
