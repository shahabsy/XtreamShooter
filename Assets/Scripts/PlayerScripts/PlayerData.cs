using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "PlayerData_", menuName = "Game/Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Identity")]
    public string playerName;
    public int id;
    public string displayName;
    public string prefabResourcePath;
    public string iconResourcePath;

    [Header("Stats")]
    public float maxHealth = 20000f;
    public float maxShield = 5500f;
    public float shieldRegen = 200f;
    public float shieldRegenInterval = 0.05f;
    public float shieldRegenStartTime = 3f;
    public float maxEnergy = 65000f;
    public float energyRegen = 20f;
    public float energyRegenInterval = 0.05f;
    public float speed = 10f;
    public float acceleration = 10f;
    public float collisionDamage = 4000f;
    public float collisionDamageCooldown = 0.5f;
    public float invincibilityDuration = 1.5f;

    [Header("Weapons")]
    public List<PlayerShootBehavior> startingWeapons;

    [Header("Visuals")]
    public Sprite sprite;
    public Vector2 spriteScale = Vector2.one;
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;

    [Header("VFX")]
    public GameObject hitVFX;
    public AudioClip hitSFX;
    public GameObject deathVFX;
    public AudioClip deathSFX;
    public GameObject shieldHitVFX;
    public AudioClip shieldHitSFX;
    public GameObject shieldBreakVFX;
    public AudioClip shieldBreakSFX;

    [Header("Spawn Points")]
    public Vector2 shotSpawnOffset;
    public List<ThrusterSpawnData> thrusterSpawns;

    [Header("Wquiment (future)")]
    public List<string> equipmentSlots;
    public List<string> sampleWeapons;
    public List<string> sampleMods;

    [Header("Economy")]
    public int cost;
    public int sellPrice;
}

[System.Serializable]
public struct ThrusterSpawnData
{
    public GameObject vfxPrefab;
    public Vector2 offset;
}
