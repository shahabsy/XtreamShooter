using UnityEngine;

[CreateAssetMenu(fileName = "EnemyShootBehavior", menuName = "Game/Enemy/EnemyShootBehavior")]
public abstract class EnemyShootBehavior : ScriptableObject
{
    public float fireRate = 1f;
    public BulletConfig bullet;

    public bool shootImmediately = true;
    
    protected const int MaxBulletsPerFrame = 50;
    protected Enemy enemy;
    protected Transform firePoint;
    protected float elapsedTime;

    

    public virtual void Initialize(Enemy ower, Transform point)
    {
        enemy = ower;
        firePoint = point;    
        elapsedTime = 0f;
    }

    public abstract void TryShoot(float deltaTime);
    public virtual void ForceFire() { }

    protected virtual void SpawnBullet(Vector2 direction)
    {
        if (string.IsNullOrEmpty(bullet.poolTag)) return;
        GameObject obj = ObjectPooler.Instance.SpawnFromPool(bullet.poolTag, firePoint.position, Quaternion.identity);
        if(obj == null) return;

        EnemyProjectile projectile = obj.GetComponent<EnemyProjectile>();
        if(projectile != null)
        {
            projectile.Initialize(direction * bullet.speed, bullet.lifeTime, bullet.damage, bullet.poolTag, enemy);
        }
    }

    protected Vector2 GetForwardDirection()
    {
        return -enemy.transform.right;
    }
}
