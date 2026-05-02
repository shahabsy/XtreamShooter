using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyData_", menuName = "Game/Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;
    [Header("Stats")]
    public float health = 30f;
    public float moveSpeed = 2f;
    public int scoreValue = 10;

    [Header("AI Behavior")]
    public EnemyAIBehavior aiBehavior;

    [Header("Shooting Settings (multiple allowed")]
    public List<EnemyShootBehavior> shootBehavior;

    [Header("Visuals")]
    public Sprite sprite;
    public Vector2 spriteScale = Vector2.one;
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;

    [Header("Prefab (required for spawning")]
    public GameObject prefab;
}
