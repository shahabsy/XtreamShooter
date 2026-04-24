using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    public float damage { get; private set; } = 10f;
    private string poolTag;

    void OnEnable()
    {
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void ReturnToPool()
    {
        CancelInvoke();
        gameObject.SetActive(false);
    }

    public void OnHit()
    {
        ReturnToPool();
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;
    public void SetPoolTag(string tag) => poolTag = tag;
    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            ReturnToPool();
            return;
        }

        ReturnToPool();
    }
    */
}
