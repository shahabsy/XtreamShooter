using UnityEngine;

[CreateAssetMenu(fileName = "SpawnEnemySummon", menuName = "Game/Boss/SpawnEnemySummon")]
public class SpawnEnemySummon : BossSummonBehavior
{
    public EnemyData enemyToSpawn;

    public override void TrySummon(float deltaTime)
    {
        timer += deltaTime;
        if (timer >= summonInterval)
        {
            timer -= summonInterval;
            SummonEnemy();
        }
    }

    private void SummonEnemy()
    {
        if (enemyToSpawn == null || enemyToSpawn.prefab == null) return;
        GameObject enemyObj = Instantiate(enemyToSpawn.prefab, spawnPoint.position, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
            enemy.Initialize(enemyToSpawn);
    }
}
