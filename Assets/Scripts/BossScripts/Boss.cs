using UnityEngine;
using System;
using System.Linq;
using System.Collections;

public class Boss : MonoBehaviour, IDamageable
{
    public event Action<float> OnHealthChanged;
    public event Action<int> OnPhaseChanged;
    public event Action OnDefeat;

    [Header("Data")]
    [SerializeField] private BossData data;

    private float currentHealth;
    private int currentPhase = -1;
    private bool isDefeated = false;
    private bool registered = false;
    private bool initialized = false;

    public Transform firePoint;
    private float nextFireTime;
    private SpriteRenderer spriteRenderer;

    public float HealthNormalized => currentHealth / data.maxHealth;

    private void Awake()
    {
        if (firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(transform);
        }
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!initialized)
        {
            enabled = false;
        }
    }

    public void Initialize(BossData bossData)
    {
        if (initialized) return;

        data = bossData;
        currentHealth = data.maxHealth;
        SetupVisuals();
        //PositionFirePoint();
        SortPhasesDescending();
        initialized = true;

        EnemySpawner.Instance?.RegisterEnemy();
        registered = true;

        StartBehavior();
    }

    private void SetupVisuals()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite = data.sprite;
        spriteRenderer.sortingLayerName = data.sortingLayerName;
        spriteRenderer.sortingOrder = data.orderInLayer;
        transform.localScale = data.spriteScale;
    }

    private void PositionFirePoint()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            float rightEdge = spriteRenderer.bounds.size.x * transform.localScale.x / 2f;
            firePoint.localPosition = new Vector3(rightEdge + 0.3f, 0f, 0f);
        }
        else
        {
            firePoint.localPosition = new Vector3(0.8f, 0f, 0f);
        }
    }
    private void SortPhasesDescending()
    {
        if (data.phases == null && data.phases.Length <= 1) return;
        
        data.phases = data.phases.OrderByDescending(p => p.healthThreshold).ToArray();
    }

    private void StartBehavior()
    {
        CheckPhaseTransition(true);
    }
    // Update is called once per frame
    void Update()
    {
        if (isDefeated || data == null) return;

        transform.Translate(Vector2.left * data.moveSpeed * Time.deltaTime);

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / data.fireRate);
            Shoot();
        }

        if (transform.position.x < data.leftBoundary)
        {
            Defeat();
        }
    }
    void Shoot()
    {
        if (ObjectPooler.Instance != null && !string.IsNullOrEmpty(data.bulletPoolTag))
        {
            GameObject bullet = ObjectPooler.Instance.SpawnFromPool("EnemyBullet", firePoint.position, Quaternion.identity);
            if (bullet != null)
            {
                var proj = bullet.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.SetSpeed(-data.bulletSpeed);
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDefeated || !initialized) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);
        OnHealthChanged?.Invoke(HealthNormalized);

        CheckPhaseTransition();

        if(currentHealth <= 0f)
        {
            Defeat();
        }
    }

    private void CheckPhaseTransition(bool forceInitial = false)
    {
        if (data.phases == null || data.phases.Length == 0)
            return;

        for(int i = 0; i < data.phases.Length; i++)
        {
            if(HealthNormalized <= data.phases[i].healthThreshold && currentPhase < i)
            {
                EnterPhase(i);
                break;
            }
        }
    }

    public  void EnterPhase(int phaseIndex) 
    {
        currentPhase = phaseIndex;
        OnPhaseChanged?.Invoke(phaseIndex);

        string taunt = data.phases[phaseIndex].dialog;
        if(!string.IsNullOrEmpty(taunt))
        {
            //StageAction tempAction = new StageAction();
            //tempAction.type = StageAction.ActionType.Dialog;
            //tempAction.text = taunt;
            //tempAction.characterName = data.bossName;
            //DialogManager.Instance?.ShowDialog(tempAction, null);
            Debug.Log($"Phase {phaseIndex} : {data.bossName} says {taunt}");
        }
    }

    private void Defeat()
    {
        if (isDefeated) return;
        isDefeated = true;

        UnresiterIfNeeded();
        OnDefeat?.Invoke();
        StartCoroutine(DeathSequence());
    }
    private void UnresiterIfNeeded()
    {
        if (registered)
        {
            EnemySpawner.Instance?.UnregisterEnemy();
            registered = false;
        }
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        UnresiterIfNeeded();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Boss collided with {other.gameObject.name}");
        if (other.CompareTag("PlayerBullet"))
        {
            Projectile proj = other.GetComponent<Projectile>();
            if (proj != null)
            {
                TakeDamage(proj.damage);
                proj.OnHit();
                Debug.Log($"Boss hit by projectile dealing {proj.damage} damage");
            }
        }

    }
}
