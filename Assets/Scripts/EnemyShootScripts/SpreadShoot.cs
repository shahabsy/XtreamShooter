using UnityEngine;
[CreateAssetMenu(fileName = "SpreadShoot", menuName = "Game/Enemy/Shoot/SpreadShoot")]
public class SpreadShoot : EnemyShootBehavior
{
    public int bulletCount = 3;
    public float spreadAngle = 30f;
    public float angleOffset = -45f;//Extra rotation applied to every bullet to facing forward.
    public override void TryShoot(float deltaTime)
    {
        float interval = fireRate > 0f ? 1f / fireRate : float.MaxValue;
        elapsedTime += deltaTime;
        while(elapsedTime >= interval)
        {
            elapsedTime -= interval;
            FireWithSafety();
        }
    }
    public override void ForceFire()
    {
        FireWithSafety();
    }
    private void FireWithSafety()
    {
        int bulletSpawned = 0;
        Vector2 baseDir = GetForwardDirection();
        float startAngle = -spreadAngle * 0.5f;
        float step = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0;
        for(int i = 0; i < bulletCount; i++)
        {
            if (bulletSpawned >= MaxBulletsPerFrame) break;
            float angle = startAngle + i * step + angleOffset;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector2 dir = rot * baseDir;
            SpawnBullet(dir);
            bulletSpawned++;
        }
    }
}
