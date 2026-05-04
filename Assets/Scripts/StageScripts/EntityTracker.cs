using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class EntityTracker : MonoBehaviour
{
    public static EntityTracker Instance { get; private set; }

    private readonly HashSet<Enemy> enemies = new HashSet<Enemy>();
    private readonly HashSet<Boss> bosses = new HashSet<Boss>();

    private readonly Dictionary<string, HashSet<Enemy>> waveMap = new Dictionary<string, HashSet<Enemy>>();
    private readonly Dictionary<Enemy, string> enemyToWave = new Dictionary<Enemy, string>();

    public int EnemyCount => enemies.Count;
    public bool HasAnyEnemy => enemies.Count > 0;
    public bool HasBoss => bosses.Count > 0;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        if(!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        enemies.Remove(enemy);
        // remove mapping from enemyToWave if present
        if(enemyToWave.TryGetValue(enemy, out var waveId))
        {
            enemyToWave.Remove(enemy);
            if(waveMap.TryGetValue(waveId, out var set))
            {
                set.Remove(enemy);
                if(set.Count == 0)
                {
                    waveMap.Remove(waveId);
                }
            }
        }
    }

    public void AssignEnemyToWave(Enemy enemy, string waveId)
    {
        if (enemy == null || string.IsNullOrEmpty(waveId)) return;
        
        if(!enemies.Contains(enemy)) enemies.Add(enemy);

        if (!waveMap.TryGetValue(waveId, out var set))
        {
            set = new HashSet<Enemy>();
            waveMap[waveId] = set;
        }
        set.Add(enemy);
        enemyToWave[enemy] = waveId;
    }

    public bool HasEnemiesInWave(string waveId)
    {
        if (string.IsNullOrEmpty(waveId)) return false;
        return waveMap.TryGetValue(waveId, out var set) && set.Count > 0;
    }

    public void ClearWave(string waveId)
    {
        if (string.IsNullOrEmpty(waveId)) return;
        if (!waveMap.TryGetValue(waveId, out var set)) return;
        
        foreach(var e in new List<Enemy>(set))
        {
            Destroy(e.gameObject);
        }
        waveMap.Remove(waveId);
    }

    public void RegisterBoss(Boss boss)
    {
        if (boss == null) return;
        bosses.Add(boss);
    }

    public void UnregisterBoss(Boss boss)
    {
        if (boss == null) return;
        bosses.Remove(boss);
    }
    public void ResetTracker()
    {
        ClearAllEntities();
    }
    public void ClearAllEntities()
    {
        var enemiesCopy = new List<Enemy>(enemies);
        var bossesCopy = new List<Boss>(bosses);

        foreach (var e in enemiesCopy)
        {
            if (e != null)
                Destroy(e.gameObject);
        }
        foreach (var b in bossesCopy)
        {
            if (b != null)
                Destroy(b.gameObject);
        }
        enemies.Clear();
        bosses.Clear();
        waveMap.Clear();
        enemyToWave.Clear();
    }
    
    private void LateUpdate()
    {
        enemies.RemoveWhere(e => e == null);
        bosses.RemoveWhere(b => b == null);

        var emptyWaves = new List<string>();
        foreach(var kvp in waveMap)
        {
            kvp.Value.RemoveWhere(e => e == null);
            if (kvp.Value.Count == 0) emptyWaves.Add(kvp.Key);
        }
        foreach(var waveId in emptyWaves)
        {
            waveMap.Remove(waveId);
        }
    }
    
}
