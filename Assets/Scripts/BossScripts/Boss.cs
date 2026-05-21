using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


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

    private List<BossShootBehavior> runtimeShoots = new List<BossShootBehavior>();
    private List<BossSummonBehavior> runtimeSummons = new List<BossSummonBehavior>();

    public Transform[] firePoints; // set this in inspector
    public Transform[] summonSpawnPoints;

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
        //Debug.Log($"Initializing boss with initialized {initialized}");
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
        SetupShoot();
        SetupSummon();
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
    private void SetupShoot()
    {
        if (data.shootBehaviors == null) return;
        foreach(var behavior in data.shootBehaviors)
        {
            if (behavior == null) continue;
            var clone = Instantiate(behavior);

            int index = clone.firePointIndex;
            Transform point = index >= 0 && index < firePoints.Length ? firePoints[index] : firePoints[0];
            clone.Initialize(this, point);
            runtimeShoots.Add(clone);
        }
    }
    private void SetupSummon()
    {
        if (data.summonBehaviors == null) return;
        foreach(var behavior in data.summonBehaviors)
        {
            if (behavior == null) continue;
            var clone = Instantiate(behavior);
            int index = clone.spawnPointIndex;
            Transform point = (index >= 0 && index < summonSpawnPoints.Length) ? summonSpawnPoints[index] : transform;
            clone.Initialize(this, point);
            runtimeSummons.Add(clone);
        }
    }
    private void SortPhasesDescending()
    {
        if (data.phases == null || data.phases.Length <= 1) return;
        
        data.phases = data.phases
            .OrderByDescending(p => p.healthThreshold)
            .ToArray();
    }
    // Update is called once per frame
    void Update()
    {
        if (!initialized || isDefeated) return;

        transform.Translate(Vector2.left * data.moveSpeed * Time.deltaTime);

        
        foreach (var shoot in runtimeShoots)
            shoot.TryShoot(Time.deltaTime);

        foreach (var summon in runtimeSummons)
            summon.TrySummon(Time.deltaTime);


        if (transform.position.x < data.leftBoundary)
        {
            Defeat();
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
        yield return new WaitForSeconds(0.3f);
        EnemySpawner.Instance?.ClearAllEnemies();
        ClearAllEnemyBullets();
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"Boss collided with {other.gameObject.name}");
        if (other.CompareTag("PlayerBullet"))
        {
            PlayerProjectile proj = other.GetComponent<PlayerProjectile>();
            if (proj != null)
            {
                TakeDamage(proj.damage);
                //proj.();
                //Debug.Log($"Boss hit by projectile dealing {proj.damage} damage");
            }
        }

    }

    private void OnDestroy()
    {
        EntityTracker.Instance?.UnregisterBoss(this);
        foreach (var s in runtimeShoots)
            if (s != null) Destroy(s);
        runtimeShoots.Clear();
    }

    private void ClearAllEnemyBullets()
    {
        EnemyProjectile[] bullets = FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
        foreach (EnemyProjectile bullet in bullets)
        {
            bullet.ReturnToPool();
        }
    }
}
