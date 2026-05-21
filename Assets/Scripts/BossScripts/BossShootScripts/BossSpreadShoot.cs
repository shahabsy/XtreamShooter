using UnityEngine;

[CreateAssetMenu(fileName = "BossSpreadShoot", menuName = "Game/Boss/Shoot/BossSpreadShoot")]
public class BossSpreadShoot : BossShootBehavior
{
    public int bulletCount = 3;
    public float spreadAngle = 30f;
    public float angleOffset = -45f;
    public override void TryShoot(float deltaTime)
    {
        float interval = fireRate > 0f ? 1f / fireRate : float.MaxValue;
        elapsedTime += deltaTime;
        while (elapsedTime >= interval)
        {
            elapsedTime -= interval;
            Fire();
        }
    }

    public override void ForceFire() => Fire();

    private void Fire()
    {
        Vector2 baseDir = GetForwardDirection;
        float startAngle = -spreadAngle * 0.5f + angleOffset;
        float step = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + i * step;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector2 dir = rot * baseDir;
            SpawnBullet(dir);
        }
    }
}
