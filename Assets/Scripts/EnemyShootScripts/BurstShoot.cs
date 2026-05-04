using UnityEngine;
[CreateAssetMenu(fileName = "BurstShoot", menuName = "Game/Enemy/Shoot/BurstShoot")]
public class BurstShoot : EnemyShootBehavior
{
    public int shotsPerBurst = 3;
    public float burstDelay = 0.1f;
    private int remainingShots;
    private float burstTimer;

    public override void TryShoot(float deltaTime)
    {
        if(remainingShots > 0)
        {
            burstTimer += deltaTime;
            while(burstTimer > burstDelay )
            {
                burstTimer -= burstDelay;
                FireOne();
                remainingShots--;
                if(remainingShots == 0 )
                {
                    break;
                }
            }
            if (remainingShots == 0) elapsedTime = 0;
            return;
        }

        float interval = fireRate > 0f ? 1f / fireRate : float.MaxValue;
        elapsedTime += deltaTime;
        if(elapsedTime >= interval)
        {
            elapsedTime -= interval;
            remainingShots = shotsPerBurst;
            burstTimer = 0;
            FireOne();
            remainingShots--;
        }
    }
    public override void ForceFire()
    {
        for(int i = 0; i < shotsPerBurst; i++)
        {
            FireOne();
        }
    }

    private void FireOne()
    {
        Vector2 dir = GetForwardDirection();
        SpawnBullet(dir);
    }
}
