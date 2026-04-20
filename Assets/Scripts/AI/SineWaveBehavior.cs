using UnityEngine;

[CreateAssetMenu(fileName = "SineWaveBehavior", menuName = "Game/AI/SineWaveBehavior")]
public class SineWaveBehavior : EnemyAIBehavior
{
    [Header("Sine Wave Settings")]
    public Vector2 amplitudeRange = new Vector2(1f, 2f);
    public Vector2 frequencyRange = new Vector2(1f, 3f);

    private float amplitude;
    private float frequency;
    private float timeOffset;

    public override void Initialize(Enemy owner, EnemyData enemyData)
    {
        base.Initialize(owner, enemyData);

        amplitude = Random.Range(amplitudeRange.x, amplitudeRange.y);
        frequency = Random.Range(frequencyRange.x, frequencyRange.y);
        timeOffset = Random.Range(0f, 2f * Mathf.PI * 2f);
    }

    public override Vector2 GetVelocity()
    {
        float yOffset = Mathf.Sin((elapsedTime + timeOffset) * frequency) * amplitude;
        return new Vector2(-data.moveSpeed, yOffset);
    }
    public override void ResetState()
    {
        base.ResetState();
    }
}