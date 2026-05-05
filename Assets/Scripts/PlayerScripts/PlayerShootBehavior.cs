using UnityEngine;

[CreateAssetMenu(fileName = "PlayerShootBehavior", menuName = "Game/Player/ShootBehavior/PlayerShootBehavior")]
public abstract class PlayerShootBehavior : ScriptableObject
{
    [Header("Weapon")]
    public string weaponName;
    public float fireRate = 5f;
    public BulletConfig bullet;

    [Header("Energy Cost")]
    public float energyCost = 0f;

    protected const int MaxShotsPerFrame = 20;

    protected PlayerController player;
    protected Transform firePoint;
    protected float elapsedTime;

    public virtual void Initialize(PlayerController owner, Transform point)
    {
        player = owner;
        firePoint = point;
        elapsedTime = 0f;
    }

    public abstract void TryShoot(float deltaTime);
    public virtual void ForceFire() { }

    protected virtual bool CanShoot()
    {
        if(energyCost > 0f && player != null && !player.TryConsumeEnergy(energyCost))
            return false;
        return true;
    }
    protected virtual void SpawnBullet(Vector2 direction)
    {
        if (string.IsNullOrEmpty(bullet.poolTag)) return;
        GameObject obj = ObjectPooler.Instance.SpawnFromPool(bullet.poolTag, firePoint.position, Quaternion.identity);
        if (obj == null) return;

        PlayerProjectile projectile = obj.GetComponent<PlayerProjectile>();
        if(projectile != null)
        {
            Vector2 velocity = direction * bullet.speed;
            projectile.Initialize(velocity, bullet.lifeTime, bullet.damage, bullet.poolTag);
        }
    }

    protected Vector2 GetForwardDirection() => player.transform.right;
}
