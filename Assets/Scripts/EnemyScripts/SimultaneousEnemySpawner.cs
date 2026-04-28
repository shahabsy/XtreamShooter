using UnityEngine;

public class SimultaneousEnemySpawner : BaseEnemySpawner
{
    public override void StartSpawn()
    {
        if(!IsPatternValid())
        {
            isDone = true;
            return;
        }
        foreach(EnemySpawnEntry entry in pattern.spawnSequence)
        {
            Vector2 spawnPos = GetRandomEdgePosition();
            SpawnEnemy(entry, spawnPos.x, spawnPos.y);
        }
        isDone = true;
    }
    private Vector2 GetRandomEdgePosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector2.zero;

        //Determin screen bounds in world coords
        Vector3 bottomLeft = cam.ViewportToWorldPoint(Vector3.zero);
        Vector3 topRight = cam.ViewportToWorldPoint(Vector3.one);
        SpawnEdge chosenEdge;
        if(pattern.allowedEdges != null && pattern.allowedEdges.Length > 0)
        {
            int idx = prng.GetPseudoRandomInt(0, pattern.allowedEdges.Length);
            chosenEdge = pattern.allowedEdges[idx];
        }
        else
        {
            chosenEdge = SpawnEdge.Right; // Fallback
        }
        float x, y;
        switch(chosenEdge)
        {
            case SpawnEdge.Top:
                x = prng.GetPseudoRandomNumber(bottomLeft.x, topRight.x);
                y = topRight.y;
                break;
            case SpawnEdge.Bottom:
                x = prng.GetPseudoRandomNumber(bottomLeft.x, topRight.x);
                y = bottomLeft.y;
                break;
            case SpawnEdge.Right:
            default:
                x = topRight.x;
                y = prng.GetPseudoRandomNumber(bottomLeft.y, topRight.y);
                break;
        }
        return new Vector2(x, y);
    }
}
