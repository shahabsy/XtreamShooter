using System;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public float health = 100f;
    public event Action OnDefeat;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            OnDefeat?.Invoke();
            Destroy(gameObject);
        }
    }
    
    public void StartBossFight()
    {
        enabled = true;
    }
}
