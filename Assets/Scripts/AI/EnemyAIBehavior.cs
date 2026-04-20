using UnityEngine;

public abstract class EnemyAIBehavior : ScriptableObject
{
    protected Enemy enemy;
    protected EnemyData data;

    protected float elapsedTime;

    public virtual void Initialize(Enemy owner, EnemyData enemyData)
    {
        enemy = owner;
        data = enemyData;
        elapsedTime = 0f;
    }

    public virtual void UpdateLogic(float deltaTime)
    {
        elapsedTime += deltaTime;
    }

    public abstract Vector2 GetVelocity();

    public virtual void OnDeath() { }

    public virtual void ResetState()
    {
        elapsedTime = 0f;
    }

}
