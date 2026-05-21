using UnityEngine;

[CreateAssetMenu(fileName = "BossSpiralShoot", menuName = "Game/Boss/Shoot/BossSpiralShoot")]
public class BossSpiralShoot : BossShootBehavior
{
    public int bulletsPerShot = 1;
    public float angleStep = 15f;
    private float currentAngle = 0f;

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
        Vector2 baseDir = GetForwardDirection;
        for(int i = 0; i < bulletsPerShot; i++)
        {
            float angle = currentAngle + i * angleStep;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector2 dir = rot * baseDir;
            SpawnBullet(dir);
        }
        currentAngle += angleStep;
    }

}
