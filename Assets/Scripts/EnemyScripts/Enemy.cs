using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] private EnemyData data;
    [Header("Spawn Info")]
    public DataEnemySpawnPattern sourceSpawnPattern;
    public bool isElite;
    public bool isBoss;

    private bool isDead = false;
    private float currentHealth;
    public Transform firePoint;
    private Rigidbody2D rb;

    private EnemyAIBehavior runtimeAIBehavior;
    private List<EnemyShootBehavior> runtimeShoots = new List<EnemyShootBehavior>();

    private bool[] weaponsPaused;

    private bool isEntering;
    private Vector2 entryTarget;
    private float entryDuration;
    private float entryElapsed;

    public event Action<Enemy> OnDeath;

    private Vector2 slideStart;
    //private float softBoundaryForce = 5f;

    private float leftBoundary;
    private bool isExitingFlag = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void OnEnable()
    {
        EntityTracker.Instance?.RegisterEnemy(this);
    }
    private void OnDisable()
    {
        EntityTracker.Instance?.UnregisterEnemy(this);
        if (runtimeAIBehavior != null) Destroy(runtimeAIBehavior);
        foreach (var s in runtimeShoots) if (s != null) Destroy(s);
        runtimeShoots.Clear();
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            leftBoundary = Camera.main.ViewportToWorldPoint(Vector3.zero).x - 1f;
            //Debug.Log($"LeftBoundary for enemy: {leftBoundary}");

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

        //SetupFirePoint();
        SetupAIBehavior();
        SetupShoot();

        EntityTracker.Instance?.RegisterEnemy(this);
    }
    private void SetupAIBehavior()
    {
        if (data.aiBehavior == null) return;

        runtimeAIBehavior = Instantiate(data.aiBehavior);
        runtimeAIBehavior.hideFlags = HideFlags.HideAndDontSave;
        runtimeAIBehavior.Initialize(this, data);
    }
    private void SetupShoot()
    {
        if (data.shootBehavior == null) return;
        foreach (EnemyShootBehavior shootBehavior in data.shootBehavior)
        {
            if (shootBehavior == null) continue;
            EnemyShootBehavior clone = Instantiate(shootBehavior);
            clone.Initialize(this, firePoint);
            runtimeShoots.Add(clone);
        }
        weaponsPaused = new bool[runtimeShoots.Count];
    }

    private void SetupFirePoint()
    {
        if (firePoint != null) return;

        firePoint = new GameObject("FirePoint").transform;
        firePoint.SetParent(transform);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
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
        slideStart = startPos;
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

        if (isEntering)
        {
            entryElapsed += deltaTime;
            float t = Mathf.Clamp01(entryElapsed / entryDuration);
            transform.position = Vector2.Lerp(transform.position, entryTarget, t);
            if (t >= 1f) isEntering = false;
            return;
        }
        // Update AI behavior
        runtimeAIBehavior?.UpdateLogic(deltaTime);
        //Vector2 desiredVelocity = runtimeAIBehavior?.GetVelocity() ?? (-transform.right * data.moveSpeed);

        //rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, desiredVelocity, data.acceleration * deltaTime);
        for (int i = 0; i < runtimeShoots.Count; i++)
        {
            if (!weaponsPaused[i])
                runtimeShoots[i].TryShoot(deltaTime);
        }

        // Off screen check and other stuff if()
        if (!isExitingFlag && transform.position.x < leftBoundary)
        {
            Debug.Log("Die is called at boundary.");
            Die();
        }
    }
    private void FixedUpdate()
    {
        if (isDead || isEntering) return;
        Vector2 desiredVel = runtimeAIBehavior?.GetVelocity() ?? (-transform.right * data.moveSpeed);
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, desiredVel, data.acceleration * Time.fixedDeltaTime);
    }
    public void BeginExit()
    {
        isExitingFlag = true;
    }

    public void SetWeaponsPaused(List<int> indices, bool paused)
    {
        foreach(int index in indices)
            if(index >= 0 && index < weaponsPaused.Length)
                weaponsPaused[index] = paused;
    }

    public void FireWeaponImmediately(int index)
    {
        if (index >= 0 && index < runtimeShoots.Count)
            runtimeShoots[index].ForceFire();
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

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        runtimeAIBehavior?.OnDeath();
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    public Transform CurrentTarget => GameManager.Instance?.CurrentPlayer?.transform;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerBullet")) return;

        Projectile proj = other.GetComponent<Projectile>();
        if (proj != null)
        {
            TakeDamage(proj.damage);
            //proj.OnHit();
        }
    }
}
