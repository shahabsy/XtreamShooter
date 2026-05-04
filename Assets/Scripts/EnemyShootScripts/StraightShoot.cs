using UnityEngine;
[CreateAssetMenu(fileName = "StraightShoot", menuName = "Game/Enemy/Shoot/StraightShoot")]
public class StraightShoot : EnemyShootBehavior
{
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
    public override void ForceFire()
    {
        Fire();
    }
    private void Fire()
    {
        Vector2 dir = GetForwardDirection();
        SpawnBullet(dir);
    }
}
