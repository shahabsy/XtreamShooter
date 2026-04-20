using UnityEngine;

[CreateAssetMenu(menuName = "Game/AI/Straight Behavior")]
public class StraightBehavior : EnemyAIBehavior
{
    public override Vector2 GetVelocity()
    {
        return Vector2.left * data.moveSpeed;
    }
    
}
