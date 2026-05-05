using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    public static PlayerController Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private PlayerData data;

    private float currentHealth;
    private float currentShield;
    private float currentEnergy;
    private bool isInvincible = false;
    private bool isDead = false;
    private float lastDamageTime = -999f;

    private float shieldRegenTimer;
    private float energyRegenTimer;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private List<PlayerShootBehavior> runtimeWeapons = new List<PlayerShootBehavior>();

    private Transform firePoint;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnShieldChanged;
    public event Action<float, float> OnEnergyChanged;
    public event Action OnPlayerDeath;

    //private event Action<string> onTriggerEvent;

    //public void RegisterTriggerListener(Action<string> listener) => onTriggerEvent += listener;
    //public void UnregisterTriggerListener(Action<string> listener) => onTriggerEvent -= listener;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void Start()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        ApplyVisuals();
        SetupFirePoint();
        SetupWeapons();
        ResetStats();

        UIManager.Instance?.UpdatePlayerHealth(currentHealth, data.maxHealth);
        UIManager.Instance?.UpdatePlayerShield(currentShield, data.maxShield);
        UIManager.Instance?.UpdatePlayerEnergy(currentEnergy, data.maxEnergy);
    }
    private void SetupFirePoint()
    {
        firePoint = new GameObject("FirePoint").transform;
        firePoint.SetParent(transform);
        firePoint.localPosition = data.shotSpawnOffset;
    }
    private void SetupWeapons()
    {
        if (data.startingWeapons == null) return;
        foreach(var weapon in data.startingWeapons)
        {
            if(weapon == null) continue;
            var clone = Instantiate(weapon);
            clone.Initialize(this, firePoint);
            runtimeWeapons.Add(clone);
        }
    }
    private void ApplyVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if(sr != null && data.sprite != null)
        {
            sr.sprite = data.sprite;
            sr.sortingLayerName = data.sortingLayerName;
            sr.sortingOrder = data.orderInLayer;
        }
        transform.localScale = new Vector3(data.spriteScale.x, data.spriteScale.y, 1f);
    }
    public void ResetPlayer()
    {
        ResetStats();
        transform.position = Vector3.zero;
        //Reset weapons if needed
        StopAllCoroutines();
    }
    private void ResetStats()
    {
        currentHealth = data.maxHealth;
        currentShield = data.maxShield;
        currentEnergy = data.maxEnergy;
        isDead = false;
        isInvincible = false;
        lastDamageTime = -999f;
        shieldRegenTimer = 0f;
        energyRegenTimer = 0f;
    }

    private void Update()
    {
        if(isDead) return;

        //moveInput = new Vector2()

        UpdateRegeneration();
    }

    private void FixedUpdate()
    {
        if(isDead) return;
        Vector2 targetVelocity = moveInput * data.speed;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, data.acceleration * Time.fixedDeltaTime);
    }

    private void UpdateRegeneration()
    {
        shieldRegenTimer += Time.deltaTime;
        if(shieldRegenTimer >= data.shieldRegenInterval)
        {
            shieldRegenTimer -= data.shieldRegenInterval;
            if(currentShield < data.maxShield && Time.time > lastDamageTime + data.shieldRegenStartTime)
            {
                currentShield += data.shieldRegen;
                currentShield = Mathf.Min(currentShield, data.maxShield);
                OnShieldChanged?.Invoke(currentShield, data.maxShield);
                UIManager.Instance?.UpdatePlayerShield(currentShield, data.maxShield);
            }
        }

        energyRegenTimer += Time.deltaTime;
        if(energyRegenTimer >= data.energyRegenInterval)
        {
            energyRegenTimer -= data.energyRegenInterval;
            if(currentEnergy < data.maxEnergy)
            {
                currentEnergy += data.energyRegen;
                currentEnergy = Mathf.Min(currentEnergy, data.maxEnergy);
                OnEnergyChanged?.Invoke(currentEnergy, data.maxEnergy);
                UIManager.Instance?.UpdatePlayerEnergy(currentEnergy, data.maxEnergy);

            }
        }
    }

    public bool TryConsumeEnergy(float amount)
    {
        if(currentEnergy < amount) return false;
        currentEnergy -= amount;
        OnEnergyChanged?.Invoke(currentEnergy, data.maxEnergy);
        UIManager.Instance?.UpdatePlayerEnergy(currentEnergy, data.maxEnergy);
        return true;
    }
    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;
        lastDamageTime = Time.time;
        float remainingDamage = damage;

        if(currentShield > 0)
        {
            float shieldAbsord = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldAbsord;
            remainingDamage -= shieldAbsord;
            OnShieldChanged?.Invoke(currentShield, data.maxShield);
            UIManager.Instance?.UpdatePlayerShield(currentShield, data.maxShield);
            PlayShieldHitEffect();
            if (currentShield <= 0) PlayShieldBreakEffect();
        }
        if(remainingDamage > 0)
        {
            currentHealth -= remainingDamage;
            OnHealthChanged?.Invoke(currentHealth, data.maxHealth);
            UIManager.Instance?.UpdatePlayerHealth(currentHealth, data.maxHealth);
            PlayHitEffect();
            if (currentHealth <= 0) Die();
            else StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(data.invincibilityDuration);
        isInvincible = false;
    }
    private void Die()
    {
        if(isDead) return;
        isDead = true;
        OnPlayerDeath?.Invoke();
        PlayDeathEffect();
        Destroy(gameObject, 1f);
    }

    private void PlayHitEffect()
    {
        if (data.hitVFX != null) Instantiate(data.hitVFX, transform.position, Quaternion.identity);
        if (data.hitSFX != null) AudioManager.Instance?.PlaySFX(data.hitSFX, transform.position);
    }

    private void PlayShieldHitEffect()
    {
        if(data.shieldHitVFX != null) Instantiate(data.shieldHitVFX, transform.position, Quaternion.identity);
        if (data.shieldHitSFX != null) AudioManager.Instance?.PlaySFX(data.shieldHitSFX, transform.position); 
    }

    private void PlayShieldBreakEffect()
    {
        if (data.shieldBreakVFX != null) Instantiate(data.shieldBreakVFX, transform.position, Quaternion.identity);
        if (data.shieldBreakSFX != null) AudioManager.Instance?.PlaySFX(data.shieldBreakSFX, transform.position);
    }

    private void PlayDeathEffect()
    {
        if(data.deathVFX != null) Instantiate(data.deathVFX, transform.position, Quaternion.identity);
        if (data.deathSFX != null) AudioManager.Instance?.PlaySFX(data.deathSFX, transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("EnemyBullet"))
        {
            var projectile = other.GetComponent<PlayerProjectile>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage);
            }
        }
        else if(other.CompareTag("Enemy"))
        {
            TakeDamage(data.collisionDamage);
            StartCoroutine(CollisionCooldown());
        }
    }
    private IEnumerator CollisionCooldown()
    {
        yield return new WaitForSeconds(data.collisionDamageCooldown);
    }
    public void Respawn()
    {
        ResetStats();
        isDead = false;
        isInvincible = true;
        transform.position = Vector3.zero;
        StartCoroutine(InvincibilityRoutine());
    }
}
