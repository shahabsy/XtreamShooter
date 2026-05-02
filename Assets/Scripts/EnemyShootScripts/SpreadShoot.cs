using UnityEngine;
[CreateAssetMenu(fileName = "SpreadShoot", menuName = "Game/Enemy/SpreadShoot")]
public class SpreadShoot : EnemyShootBehavior
{
    public int bulletCount = 3;
    public float spreadAngle = 30f;
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
        Vector2 baseDir = GetForwardDirection();
        float startAngle = -spreadAngle / 2f;
        float step = bulletCount > 1 ? startAngle / (bulletCount - 1) : 0;
        for(int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + i * step;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector2 dir = rot * baseDir;
            SpawnBullet(dir);
        }
    }
}
