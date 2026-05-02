using System;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] private EnemyData data;
    [Header("Spawn Info")]
    public DataEnemySpawnPattern sourceSpawnPattern;
    public Transform firePoint;
    

    public bool isElite;
    public bool isBoss;

    private bool isEntering;
    private Vector2 entryTarget;
    private float entryDuration;
    private float entryElapsed;

    private float currentHealth;
    
    private float leftBoundary = -12f;
    
    private bool isDead = false;

    private EnemyAIBehavior runtimeBehavior;
    private List<EnemyShootBehavior> runtimeShoots = new List<EnemyShootBehavior>();

    public event Action OnDeath;

    private void OnEnable()
    {
        EntityTracker.Instance?.RegisterEnemy(this);
    }
    private void OnDisable()
    {
        EntityTracker.Instance?.UnregisterEnemy(this);
        if(runtimeBehavior != null) Destroy(runtimeBehavior);
        foreach (var s in runtimeShoots) if (s != null) Destroy(s);
        runtimeShoots.Clear();
    }

    private void Start()
    {
        if(Camera.main != null)
        {
            leftBoundary = Camera.main.ViewportToWorldPoint(Vector3.zero).x - 1f;
            Debug.Log($"LeftBoundary for enemy: {leftBoundary}");

        }
        else
        {
            leftBoundary = -15f;
        }
    }

    public void Initialize(EnemyData enemyData, bool elite = false, bool boss = false)
    {
        data = enemyData;
        ApplyVisuals();

        if (elite)
        {
            currentHealth = data.health * 2;
            transform.localScale *= 1.2f;
        }
        else if (boss)
        {
            currentHealth = data.health * 5f;
            transform.localScale *= 1.5f;
        }
        else
            currentHealth = data.health;
        
        isDead = false;
        isElite = elite;
        isBoss = boss;

        SetupFirePoint();
        SetupBehavior();
        SetupShoot();
        
        EntityTracker.Instance?.RegisterEnemy(this);
    }
    private void SetupShoot()
    {
        if (data.shootBehavior == null) return;
        foreach(EnemyShootBehavior shootBehavior in data.shootBehavior)
        {
            if (shootBehavior == null) continue;
            EnemyShootBehavior clone = Instantiate(shootBehavior);
            clone.Initialize(this, firePoint);
            runtimeShoots.Add(clone);
        }
    }
    private void SetupBehavior()
    {
        if (data.aiBehavior == null) return;

        runtimeBehavior = Instantiate(data.aiBehavior);
        runtimeBehavior.hideFlags = HideFlags.HideAndDontSave;
        runtimeBehavior.Initialize(this, data);
    }
    private void SetupFirePoint()
    {
        if(firePoint != null) return;

        firePoint = new GameObject("FirePoint").transform;
        firePoint.SetParent(transform);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            float spriteWidth = sr.bounds.size.x;
            firePoint.localPosition = new Vector3(spriteWidth / 2f, 0f, 0f);
        }
        else
        {
            firePoint.localPosition = Vector3.zero;
        }
    }
    private void ApplyVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null && data.sprite != null)
        {
            sr.sprite = data.sprite;
            sr.sortingLayerName = data.sortingLayerName;
            sr.sortingOrder = data.orderInLayer;
        }
        transform.localScale = new Vector3(data.spriteScale.x, data.spriteScale.y, 1f);
    }

    public void BeginSlideIn(Vector2 startPos, Vector2 targetPos, float duration)
    {
        transform.position = startPos;
        entryTarget = targetPos;
        entryDuration = duration;
        entryElapsed = 0f;
        isEntering = true;
    }
    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"Enemy {name} position {transform.position.x}, speed: {data.moveSpeed}");
        if (isDead) return;
        
        float deltaTime = Time.deltaTime;

        if(isEntering)
        {
            entryElapsed += deltaTime;
            float t = Mathf.Clamp01(entryElapsed / entryDuration);
            transform.position = Vector2.Lerp(transform.position, entryTarget, t);
            if (t >= 1f) isEntering = false;
            return;
        }
        // Update AI behavior
        runtimeBehavior?.UpdateLogic(deltaTime);
        // Get Velocity from AI behavior
        Vector2 velocity = runtimeBehavior != null
            ? runtimeBehavior.GetVelocity()
            : Vector2.left * data.moveSpeed;

        // Apply movement

        ApplyMovement(velocity, deltaTime);

        HandleShooting();

        // Off screen check and other stuff if()
        if (transform.position.x < leftBoundary)
        {
            Debug.Log("Die is called at boundary.");
            Die();
        }
    }

    private void ApplyMovement(Vector2 velocity, float deltaTime)
    {
        transform.position += (Vector3)(velocity * deltaTime);
    }

    private void HandleShooting()
    {
        // Shooting - all weapons
        foreach (var shoot in runtimeShoots)
            shoot.TryShoot(Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        runtimeBehavior?.OnDeath();
        runtimeBehavior = null;

        OnDeath?.Invoke();
        //Debug.Log($"Enemy Die: {name} (instanceId={GetInstanceID()}) calling OnDeath and Destroy.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerBullet")) return;

        Projectile proj = other.GetComponent<Projectile>();
        if (proj != null)
        {
            TakeDamage(proj.damage);
            proj.OnHit();
        }
    }
}
