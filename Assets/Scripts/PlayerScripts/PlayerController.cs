using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    //public static PlayerController Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private PlayerData data;

    [Header("Fallback Weapon (if none assigned")]
    [SerializeField] private PlayerShootBehavior defaultWeapon;

    private float currentHealth;
    private float currentShield;
    private float currentEnergy;
    private bool isInvincible = false;
    private bool isDead = false;
    private float lastDamageTime = -999f;
    private bool collisionLocked = false;

    private float shieldRegenTimer;
    private float energyRegenTimer;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private List<PlayerShootBehavior> runtimeWeapons = new List<PlayerShootBehavior>();
    private int currentWeaponIndex = 0;
    private Transform firePoint;

    private InputSystem_Actions inputActions;
    private bool isFiring = false;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnShieldChanged;
    public event Action<float, float> OnEnergyChanged;
    public event Action OnPlayerDeath;
    public event Action<int, PlayerShootBehavior> OnWeaponChanged;

    private event Action<PlayerTriggerType> onTriggerEvent;

    public void RegisterTriggerListener(Action<PlayerTriggerType> listener) => onTriggerEvent += listener;
    public void UnregisterTriggerListener(Action<PlayerTriggerType> listener) => onTriggerEvent -= listener;

    private void Awake()
    {
        //if (Instance != null && Instance != this) Destroy(gameObject);
        //else Instance = this;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;

        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        GameManager.Instance?.RegisterPlayer(this);
        ApplyVisuals();
        SetupFirePoint();
        SetupWeapons();
        ResetStats();

        UIManager.Instance?.UpdatePlayerHealth(currentHealth, data.maxHealth);
        UIManager.Instance?.UpdatePlayerShield(currentShield, data.maxShield);
        UIManager.Instance?.UpdatePlayerEnergy(currentEnergy, data.maxEnergy);
    }
    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Attack.canceled += OnAttackCanceled;
        inputActions.Player.Next.performed += OnNext;
        inputActions.Player.Previous.performed += OnPrevious;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Attack.canceled -= OnAttackCanceled;
        inputActions.Player.Next.performed -= OnNext;
        inputActions.Player.Previous.performed -= OnPrevious;
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        foreach (var w in runtimeWeapons)
            if (w != null) Destroy(w);
        runtimeWeapons.Clear();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        isFiring = true;
        FireTrigger(PlayerTriggerType.Attack);
    }
    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        isFiring = false;
    }
    private void OnNext(InputAction.CallbackContext context)
    {
        if(context.performed && runtimeWeapons.Count > 0)
        {
            runtimeWeapons[currentWeaponIndex].ResetTimer();

            currentWeaponIndex = (currentWeaponIndex + 1) % runtimeWeapons.Count;
            runtimeWeapons[currentWeaponIndex].ResetTimer();
            OnWeaponChanged?.Invoke(currentWeaponIndex, runtimeWeapons[currentWeaponIndex]);
            Debug.Log($"Switch to weapon: {runtimeWeapons[currentWeaponIndex].weaponName}");
        }
    }
    private void OnPrevious(InputAction.CallbackContext context)
    {
        if(context.performed && runtimeWeapons.Count > 0)
        {
            runtimeWeapons[currentWeaponIndex].ResetTimer();
            currentWeaponIndex = (currentWeaponIndex - 1 + runtimeWeapons.Count) % runtimeWeapons.Count;
            runtimeWeapons[currentWeaponIndex].ResetTimer();
            OnWeaponChanged?.Invoke(currentWeaponIndex, runtimeWeapons[currentWeaponIndex]);
            Debug.Log($"Switch to weapon: {runtimeWeapons[currentWeaponIndex].weaponName}");
        }
    }
    
    private void SetupFirePoint()
    {
        if(firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(transform);
        }
        firePoint.localPosition = data.shotSpawnOffset;
    }
    private void SetupWeapons()
    {
        if(data.startingWeapons != null)
        {
            foreach(var weapon in data.startingWeapons)
            {
                if (weapon == null) continue;
                var clone = Instantiate(weapon);
                clone.Initialize(this, firePoint);
                runtimeWeapons.Add(clone);
            }
        }
        if(runtimeWeapons.Count == 0 && defaultWeapon != null)
        {
            var clone = Instantiate(defaultWeapon);
            clone.Initialize(this, firePoint);
            runtimeWeapons.Add(clone);
            Debug.Log("Using default fallback weapon");
        }
        else if(runtimeWeapons.Count == 0)
        {
            Debug.LogError("No weapons availble and no fallback assigned.");
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
    private void FireTrigger(PlayerTriggerType triggerName)
    {
        onTriggerEvent?.Invoke(triggerName);
    }
    private void Update()
    {
        if (isDead) return;

        if (moveInput.sqrMagnitude > 0.01)
        {
            FireTrigger(PlayerTriggerType.Movement);
        }

        if (isFiring && runtimeWeapons.Count > 0)
        {
            runtimeWeapons[currentWeaponIndex].TryShoot(Time.deltaTime);
        }

        UpdateRegeneration();
    }
    private void FixedUpdate()
    {
        if (isDead) return;
        Vector2 targetVelocity = moveInput * data.speed;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, data.acceleration * Time.fixedDeltaTime);
    }
    private void UpdateRegeneration()
    {
        shieldRegenTimer += Time.deltaTime;
        if (shieldRegenTimer >= data.shieldRegenInterval)
        {
            shieldRegenTimer -= data.shieldRegenInterval;
            if (currentShield < data.maxShield && Time.time > lastDamageTime + data.shieldRegenStartTime)
            {
                currentShield += data.shieldRegen;
                currentShield = Mathf.Min(currentShield, data.maxShield);
                OnShieldChanged?.Invoke(currentShield, data.maxShield);
                UIManager.Instance?.UpdatePlayerShield(currentShield, data.maxShield);
            }
        }

        energyRegenTimer += Time.deltaTime;
        if (energyRegenTimer >= data.energyRegenInterval)
        {
            energyRegenTimer -= data.energyRegenInterval;
            if (currentEnergy < data.maxEnergy)
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
        if (currentEnergy < amount) return false;
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

        if (currentShield > 0)
        {
            float shieldAbsord = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldAbsord;
            remainingDamage -= shieldAbsord;
            OnShieldChanged?.Invoke(currentShield, data.maxShield);
            UIManager.Instance?.UpdatePlayerShield(currentShield, data.maxShield);
            PlayShieldHitEffect();
            if (currentShield <= 0) PlayShieldBreakEffect();
        }
        if (remainingDamage > 0)
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
        UIManager.Instance?.ShowGameOverDialog("Game Over", RestartMission, QuitGame);
        Destroy(gameObject, 0.1f);

    }

    private void RestartMission()
    {
        var stage = StageController.Instance;
        if(stage != null)
        {
            stage.RestartMission();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

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
            var projectile = other.GetComponent<EnemyProjectile>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage);
            }
        }
        else if(other.CompareTag("Enemy") && !collisionLocked)
        {
            TakeDamage(data.collisionDamage);
            StartCoroutine(CollisionCooldown());
        }
    }
    private IEnumerator CollisionCooldown()
    {
        collisionLocked = true;
        yield return new WaitForSeconds(data.collisionDamageCooldown);
        collisionLocked = false;
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
    public void Respawn()
    {
        ResetStats();
        isDead = false;
        isInvincible = true;
        transform.position = Vector3.zero;
        StartCoroutine(InvincibilityRoutine());
    }
}
