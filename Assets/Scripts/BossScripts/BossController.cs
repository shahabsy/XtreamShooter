using System;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public event Action OnDefeat;
    private float currentHealth;
    private BossDefinition data;
    private float leftBoundary = -12f;
    private bool isDead = false;

    public event Action OnDeath;

    private void Start()
    {
        
    }

    private void Update()
    {

        transform.Translate(Vector2.left * 2 * Time.deltaTime);
        if (transform.position.x < leftBoundary)
        {
            Die();
        }
    }

    public void Initialize(BossDefinition bossdata)
    {
        data = bossdata;
        currentHealth = data.health;
        // play intro music etc.
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
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
        if(other.CompareTag("PlayerBullet"))
        {
            TakeDamage(10);
            other.gameObject.SetActive(false);
        }
    }
}
