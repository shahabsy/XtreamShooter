using UnityEngine;

[CreateAssetMenu(fileName = "BossBurstShoot", menuName = "Game/Boss/Shoot/BossBurstShoot")]
public class BossBurstShoot : BossShootBehavior
{
    public int shotsPerBurst = 3;
    public float burstDelay = 0.1f;
    private int remainingShots = 0;
    private float burstTimer = 0f;

    public override void TryShoot(float deltaTime)
    {
        if (remainingShots > 0)
        {
            burstTimer += deltaTime;
            while (burstTimer >= burstDelay)
            {
                burstTimer -= burstDelay;
                FireOne();
                remainingShots--;
                if (remainingShots == 0) break;
            }
            if (remainingShots == 0) elapsedTime = 0f;
            return;
        }

        float interval = fireRate > 0f ? 1f / fireRate : float.MaxValue;
        elapsedTime += deltaTime;
        while(elapsedTime >= interval)
        {
            elapsedTime -= interval;
            remainingShots = shotsPerBurst;
            burstTimer = 0f;
            FireOne();
            remainingShots--;
        }
    }

    public override void ForceFire()
    {
        for (int i = 0; i < shotsPerBurst; i++) FireOne();
    }

    private void FireOne() => SpawnBullet(GetForwardDirection);
}
