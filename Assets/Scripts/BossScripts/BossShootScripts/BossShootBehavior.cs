using UnityEngine;

public abstract class BossShootBehavior : ScriptableObject
{
    [Header("General Settings")]
    public float fireRate = 1f; // Shots per second
    public BulletConfig bullet;

    [Header("Audio")]
    public AudioClip shootSFX;

    protected Boss boss;
    protected Transform firePoint;
    protected float elapsedTime = 0f;

    public int firePointIndex = 0; //which fire point to use.

    public virtual void Initialize(Boss owner, Transform point)
    {
        boss = owner;
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

        if(shootSFX != null)
        {
            AudioManager.Instance?.PlaySFX(shootSFX, firePoint.position);
        }

        EnemyProjectile projectile = obj.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            Vector2 velocity = direction * bullet.speed;
            projectile.Initialize(velocity, bullet.lifeTime, bullet.damage, bullet.poolTag, null);
        }
    }

    protected Vector2 GetForwardDirection => -boss.transform.right;
}
