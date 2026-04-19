using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] private EnemyData data;

    private float currentHealth;
    private float nextFireTime;
    private Transform firePoint;
    private float leftBoundary = -12f;
    private bool isDead = false;

    public event Action OnDeath;

    private void Start()
    {
        //EnemySpawner.Instance?.RegisterEnemy();
    }

    public void Initialize(EnemyData enemyData)
    {
        data = enemyData;
        currentHealth = data.health;

        if (firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(transform);
            
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null )
            {
                float spriteWidth = sr.bounds.size.x * transform.localScale.x;
                firePoint.localPosition = new Vector3(spriteWidth / 2 + 0.2f, 0, 0);
            }
            else
            {
                firePoint.localPosition = new Vector3(0.5f, 0, 0);
            }
        }
        //EnemySpawner.Instance?.RegisterEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"Enemy {name} position {transform.position.x}, speed: {data.moveSpeed}");
        if (data == null || isDead) return;
        
        transform.Translate(Vector2.left * data.moveSpeed * Time.deltaTime);

        if (transform.position.x < leftBoundary)
        {
            Die();
        }

        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + (1f / data.fireRate);
            Shoot();
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
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Projectile proj = other.GetComponent<Projectile>();
            if (proj != null)
            {
                TakeDamage(proj.damage);
                proj.OnHit();
            }
        }
    }
}
