using UnityEngine;

[CreateAssetMenu(fileName = "BossHomingShoot", menuName = "Game/Boss/Shoot/BossHomingShoot")]
public class BossHomingShoot : BossShootBehavior
{
    public float homingDelay = 0.2f;
    public float homingTurnSpeed = 360f;

    public override void TryShoot(float deltaTime)
    {
        float interval = fireRate > 0f ? 1f / fireRate : float.MaxValue;
        elapsedTime += deltaTime;
        while(elapsedTime >= interval)
        {
            elapsedTime -= interval;
            Fire();
        }
    }

    public override void ForceFire() => Fire();

    private void Fire()
    {
        Transform target = GameManager.Instance.CurrentPlayer.transform;
        if(target == null) return;

        Vector2 dir = GetForwardDirection;
        GameObject obj = ObjectPooler.Instance.SpawnFromPool(bullet.poolTag, firePoint.position, Quaternion.identity);
        if(obj == null) return;

        EnemyProjectile projectile = obj.GetComponent<EnemyProjectile>();
        if(projectile != null)
        {
            Vector2 velocity = dir * bullet.speed;
            projectile.Initialize(velocity, bullet.lifeTime, bullet.damage, bullet.poolTag, null);
            projectile.EnableHoming(target, homingDelay, homingTurnSpeed);
        }
    }
}
