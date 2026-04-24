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
    private bool initialized = false;

    private float nextFireTime;
    public Transform firePoint; // set this in inspector

    private SpriteRenderer spriteRenderer;

    public float HealthNormalized => currentHealth / data.maxHealth;

    private void OnEnable()
    {
        EntityTracker.Instance?.RegisterBoss(this);
    }
    private void OnDisable()
    {
        EntityTracker.Instance?.UnregisterBoss(this);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log($"Initializing boss with initialized {initialized}");
        if (Camera.main != null)
        {
            data.leftBoundary = Camera.main.ViewportToWorldPoint(Vector3.zero).x - 1f;
        }
        else
        {
            data.leftBoundary = -15f;
        }
    }

    public void Initialize(BossData bossData)
    {
        if (initialized) return;
        Debug.Log($"Initializing boss with name: {bossData.bossName}");
        data = bossData;
        currentHealth = data.maxHealth;

        SetupVisuals();
        SortPhasesDescending();

        initialized = true;
    }

    private void SetupVisuals()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = data.sprite;
        spriteRenderer.sortingLayerName = data.sortingLayerName;
        spriteRenderer.sortingOrder = data.orderInLayer;

        transform.localScale = data.spriteScale;
    }
    private void SortPhasesDescending()
    {
        if (data.phases == null && data.phases.Length <= 1) return;
        
        data.phases = data.phases
            .OrderByDescending(p => p.healthThreshold)
            .ToArray();
    }
    // Update is called once per frame
    void Update()
    {
        if (!initialized || isDefeated) return;

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
            GameObject bullet = ObjectPooler.Instance.SpawnFromPool(
                                                    data.bulletPoolTag,
                                                    firePoint.position,
                                                    Quaternion.identity);
            if (bullet != null)
            {
                Projectile proj = bullet.GetComponent<Projectile>();
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

    private void CheckPhaseTransition()
    {
        if (data.phases == null) return;

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
        OnDefeat?.Invoke();

        StartCoroutine(DeathSequence());
    }
    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"Boss collided with {other.gameObject.name}");
        if (other.CompareTag("PlayerBullet"))
        {
            Projectile proj = other.GetComponent<Projectile>();
            if (proj != null)
            {
                TakeDamage(proj.damage);
                proj.OnHit();
                //Debug.Log($"Boss hit by projectile dealing {proj.damage} damage");
            }
        }

    }
}
