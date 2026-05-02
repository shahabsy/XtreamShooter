using UnityEngine;
[CreateAssetMenu(fileName = "HomingShoot", menuName = "Game/Enemy/HomingShoot")]
public class HomingShoot : EnemyShootBehavior
{
    public float homingDelay = 0.2f;
    public float homingTurnSpeed = 360f; // degrees per second

    public override void TryShoot(float deltaTime)
    {
        float interval = fireRate > 0f ? 1f / fireRate : float.MaxValue;
        elapsedTime += deltaTime;
        if(elapsedTime >= interval)
        {
            elapsedTime = 0f;
            Fire();
        }
    }

    private void Fire()
    {
        Vector2 dir = GetForwardDirection();
        GameObject obj = ObjectPooler.Instance.SpawnFromPool(bullet.poolTag, firePoint.position, Quaternion.identity);
        if (obj == null) return;

        EnemyProjectile projectile = obj.GetComponent<EnemyProjectile>();
        if(projectile != null)
        {
            Vector2 velocity = dir * bullet.speed;
            projectile.Initialize(velocity, bullet.lifeTime, bullet.damage, bullet.poolTag, enemy);
            projectile.EnableHoming(PlayerController.Instance.transform, homingDelay, homingTurnSpeed);
        }
    }
}
