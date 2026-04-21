using UnityEngine;
using System.Collections.Generic;

public class EntityTracker : MonoBehaviour
{
    public static EntityTracker Instance { get; private set; }

    private readonly HashSet<Enemy> enemies = new HashSet<Enemy>();
    private readonly HashSet<Boss> bosses = new HashSet<Boss>();

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
        enemies.Add(enemy);
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        enemies.Remove(enemy);
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

    public void ClearAllEntities()
    {
        foreach(var e in enemies)
        {
            if(e != null) 
                Destroy(e.gameObject);
        }
        foreach(var b in bosses)
        {
            if(b != null) 
                Destroy(b.gameObject);
        }
        enemies.Clear();
        bosses.Clear();
    }

    private void LateUpdate()
    {
        enemies.RemoveWhere(e => e == null);
        bosses.RemoveWhere(b => b == null);
    }
}
