using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStraightShoot", menuName = "Game/Player/PlayerStraightShoot")]
public class PlayerStraightShoot : PlayerShootBehavior
{
    public override void TryShoot(float deltaTime)
    {
        float interval = fireRate > 0f ? 1f / fireRate : float.MaxValue;
        elapsedTime += deltaTime;
        int shots = 0;
        while(elapsedTime >= interval && shots < MaxShotsPerFrame)
        {
            elapsedTime -= interval;
            if(CanShoot())
            {
                Fire();
            }
            shots++;
        }
    }
    public override void ForceFire()
    {
        if (CanShoot()) Fire();
    }
    private void Fire() => SpawnBullet(GetForwardDirection());
}
