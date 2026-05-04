using UnityEngine;
[CreateAssetMenu(fileName = "SpiralShoot", menuName = "Game/Enemy/Shoot/SpiralShoot")]
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
        int bulletsSpawned = 0;
        Vector2 baseDir = GetForwardDirection();
        for (int i = 0; i < bulletsPerShot; i++)
        {
            if (bulletsSpawned >= MaxBulletsPerFrame) break;
            float angle = currentAngle + i * angleStep;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector2 dir = rot * baseDir;
            SpawnBullet(dir);
            bulletsSpawned++;
        }
        currentAngle += angleStep;
    }
}
