using UnityEngine;
[CreateAssetMenu(fileName = "SpiralShoot", menuName = "Game/Enemy/SpiralShoot")]
public class SpiralShoot : EnemyShootBehavior
{
    public int bulletsPerShot = 1;
    public float angleStep = 15f;
    private float currentAngle = 0f;
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
        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = currentAngle + i * angleStep;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector2 dir = rot * baseDir;
            SpawnBullet(dir);
        }
        currentAngle += angleStep;
    }
}
