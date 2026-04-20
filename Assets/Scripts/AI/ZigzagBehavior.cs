using UnityEngine;

[CreateAssetMenu(fileName = "ZigzagBehavior", menuName = "Game/AI/ZigzagBehavior")]
public class ZigzagBehavior : EnemyAIBehavior
{
    [Header("Randomizeable Ranges")]
    public Vector2 verticalSpeedRange = new Vector2(1f, 3f);
    public Vector2 intervalRange = new Vector2(0.5f, 1.2f);

    private float verticalSpeed;
    private float directionChangeInterval;
    private float timer;
    private float direction = 1f;

    public override void Initialize(Enemy owner, EnemyData enemyData)
    {
        base.Initialize(owner, enemyData);

        verticalSpeed = Random.Range(verticalSpeedRange.x, verticalSpeedRange.y);
        directionChangeInterval = Random.Range(intervalRange.x, intervalRange.y);

        timer = 0f;
        direction = 1f;
    }

    public override void UpdateLogic(float deltaTime)
    {
        base.UpdateLogic(deltaTime);

        timer += deltaTime;
        if (timer >= directionChangeInterval)
        {
            timer = 0f;
            direction *= -1f;
        }
    }

    public override Vector2 GetVelocity()
    {
        return new Vector2(-data.moveSpeed, direction * verticalSpeed);
    }

    public override void ResetState()
    {
        base.ResetState();
        timer = 0f;
        direction = 1f;
    }
}
