using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData_", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float health = 30f;
    public float moveSpeed = 2f;
    public float fireRate = 1f;
    public string bulletPoolTag = "EnemyBullet";
    public float bulletSpeed = 10f;
    public int scoreValue = 10;

    [Header("Visuals")]
    public Sprite sprite;
    public Vector2 spriteScale = Vector2.one;

    [Header("Rendering")]
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;
    
}
