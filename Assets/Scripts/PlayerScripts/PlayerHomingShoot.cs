using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHomingShoot", menuName = "Game/Player/PlayerHomingShoot")]
public class PlayerHomingShoot : PlayerShootBehavior
{
    public float homingDelay = 0.2f;
    public float homingTurnSpeed = 360f;

    private static Collider2D[] enemyBuffer = new Collider2D[32];
    private static ContactFilter2D enemyFilter;

    private static bool filterInitialized = false;

    private void InitFilter()
    {
        if(!filterInitialized)
        {
            enemyFilter = new ContactFilter2D();
            enemyFilter.SetLayerMask(LayerMask.GetMask("Enemies"));
            enemyFilter.useLayerMask = true;
            filterInitialized = true;
        }
    }

    public override void TryShoot(float deltaTime)
    {
        float interval = fireRate > 0f ? 1f / fireRate : float.MaxValue;
        elapsedTime += deltaTime;
        int shots = 0;
        while(elapsedTime >= interval && shots < MaxShotsPerFrame)
        {
            elapsedTime -= interval;
            if(CanShoot())
            {
                Fire();
            }
            shots++;
        }
    }
    public override void ForceFire()
    {
        if (CanShoot()) 
        {
            Fire();
        }
    }

    private void Fire()
    {
        InitFilter();
        Transform target = FindClosestEnemy();
        Vector2 dir = GetForwardDirection();
        GameObject obj = ObjectPooler.Instance.SpawnFromPool(bullet.poolTag, firePoint.position, Quaternion.identity);
        if (obj == null) return;

        PlayerProjectile projectile = obj.GetComponent<PlayerProjectile>();
        if (projectile != null)
        {
            Vector2 velocity = dir * bullet.speed;
            projectile.Initialize(velocity, bullet.lifeTime, bullet.damage, bullet.poolTag);

            if(target != null)
            {
                projectile.EnableHoming(target, homingDelay, homingTurnSpeed);
            }
        }
    }

    private Transform FindClosestEnemy()
    {
        int hitCount = Physics2D.OverlapCircle(player.transform.position, 20f, enemyFilter, enemyBuffer);
        Transform closest = null;
        float minDistSqr = float.MaxValue;
        for(int i = 0; i < hitCount; i++)
        {
            float distSqr = (enemyBuffer[i].transform.position - player.transform.position).sqrMagnitude;
            if(distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                closest = enemyBuffer[i].transform;
            }
        }
        return closest;
    }
}
