using UnityEngine;

[CreateAssetMenu(fileName = "SpawnWaveSummon", menuName = "Game/Boss/SpawnWaveSummon")]
public class SpawnWaveSummon : BossSummonBehavior
{
    public string waveName;
    public bool isElite = false;

    public override void TrySummon(float deltaTime)
    {
        timer += deltaTime;
        if (timer >= summonInterval)
        {
            timer = 0f; // Reset timer to ensure consistent intervals
            if(!string.IsNullOrEmpty(waveName))
            {
                if(isElite)
                    EnemySpawner.Instance?.SpawnEliteWaveAndReturnId(waveName);
                else
                    EnemySpawner.Instance?.SpawnWaveAndReturnId(waveName);
            }
        }
    }
}
