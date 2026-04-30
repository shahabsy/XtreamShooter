using System;
using UnityEngine;


public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] private EnemyData data;
    [Header("Spawn Info")]
    public DataEnemySpawnPattern sourceSpawnPattern;
    public bool isElite;
    public bool isBoss;

    private bool isEntering;
    private Vector2 entryTarget;
    private float entryDuration;
    private float entryElapsed;

    private float currentHealth;
    private float nextFireTime;
    private Transform firePoint;
    private float leftBoundary = -12f;
    
    private bool isDead = false;

    private EnemyAIBehavior runtimeBehavior;

    public event Action OnDeath;

    private void OnEnable()
    {
        EntityTracker.Instance?.RegisterEnemy(this);
    }
    private void OnDisable()
    {
        EntityTracker.Instance?.UnregisterEnemy(this);
    }

    private void Start()
    {
        if(Camera.main != null)
        {
            leftBoundary = Camera.main.ViewportToWorldPoint(Vector3.zero).x - 1f;
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
        
        EntityTracker.Instance?.RegisterEnemy(this);
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
            Die();
        }
    }

    private void ApplyMovement(Vector2 velocity, float deltaTime)
    {
        transform.position += (Vector3)(velocity * deltaTime);
    }

    private void HandleShooting()
    {
        if (data.fireRate <= 0) return;

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / data.fireRate);
            Shoot();
        }
    }

    void Shoot()
    {
        if (ObjectPooler.Instance == null) return;
        if (string.IsNullOrEmpty(data.bulletPoolTag)) return;

        GameObject bullet = ObjectPooler.Instance.SpawnFromPool(
            data.bulletPoolTag, firePoint.position, Quaternion.identity);
        if (bullet == null) return;
        var proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetSpeed(-data.bulletSpeed);
        }
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
