using UnityEngine;

[CreateAssetMenu(fileName = "BossStraightShoot", menuName = "Game/Boss/Shoot/StraightShoot")]
public class BossStraightShoot : BossShootBehavior
{
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

    private void Fire() => SpawnBullet(GetForwardDirection);
}
