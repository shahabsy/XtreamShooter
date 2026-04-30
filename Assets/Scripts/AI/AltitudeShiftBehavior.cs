using UnityEngine;
[CreateAssetMenu(fileName = "AltitudeShiftBehavior", menuName = "Game/AI/AltitudeShift")]
public class AltitudeShiftBehavior : EnemyAIBehavior
{
    [Header("Movement - Moveing Left and Adjusting Y to follow player")]
    public float horizontalSpeed = -2f;
    public float verticalFollowSpeed = 1f;
    public override Vector2 GetVelocity()
    {
        float yTarget = 0f;
        if(enemy != null && PlayerController.Instance != null )
        {
            float playerY = PlayerController.Instance.transform.position.y;
            float currentY = enemy.transform.position.y;
            float deltaY = playerY - currentY;

            float yMove = Mathf.Clamp(deltaY * verticalFollowSpeed, -verticalFollowSpeed, verticalFollowSpeed);
            yTarget = yMove;
        }
        return new Vector2(horizontalSpeed, yTarget);
    }
}
