using System;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public event Action OnDefeat;
    private float currentHealth;
    private BossDefinition data;
    private bool autoDefeat = true;

    private void Start()
    {
        if (autoDefeat)
        {
            Invoke(nameof(AutoDefeat), 5f);
            Debug.Log($"Boss will auto-defeat in 5 secs.");
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
            OnDefeat?.Invoke();
            Destroy(gameObject);
        }
    }

    public void AutoDefeat()
    {
        OnDefeat?.Invoke();
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
