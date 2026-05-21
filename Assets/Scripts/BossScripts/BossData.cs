using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossData_", menuName = "Game/BossData")]
public class BossData : ScriptableObject
{
    [Header("Identity")]
    public string bossName;
    public string bossId;

    [Header("Combat Stats")]
    public float maxHealth = 500f;
    public float moveSpeed = 1f;

    [Header("Visuals")]
    public Sprite sprite;
    public Vector2 spriteScale = Vector2.one;
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;

    [Header("ShootingBehaviors (multiple allowed)")]
    public List<BossShootBehavior> shootBehaviors;

    [Header("SummonBehaviors (multiple allowed)")]
    public List<BossSummonBehavior> summonBehaviors;

    [Header("Phase Transitions")]
    public BossPhaseData[] phases; // Array of phase data, ordered by health threshold descending

    [Header("Boundaries")]
    public float leftBoundary = -12f;
}

[System.Serializable]
public class  BossPhaseData
{
    [UnityEngine.Range(0f, 1f)]
    public float healthThreshold; // Percentage of max health to trigger this phase
    [TextArea]
    public string dialog; // Dialog to show when this phase starts
}
