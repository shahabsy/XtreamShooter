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

    [Header("Attack Settings")]
    public float fireRate = 1f;
    public string bulletPoolTag = "Enemybullet";
    public float bulletSpeed = 5f;

    [Header("Phase Transitions")]
    public BossPhaseData[] phases; // Array of phase data, ordered by health threshold descending

    [Header("Boundaries")]
    public float leftBoundary = -12f;
}

[System.Serializable]
public class  BossPhaseData
{
    [Range(0f, 1f)]
    public float healthThreshold; // Percentage of max health to trigger this phase
    [TextArea]
    public string dialog; // Dialog to show when this phase starts
}
