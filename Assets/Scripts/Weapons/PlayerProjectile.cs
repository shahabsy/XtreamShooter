using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerProjectile : MonoBehaviour
{
    public float acceleration = 0f;
    public bool rotateToVelocity = true;
    public float damage { get; private set; }

    private Rigidbody2D rb;
    private float lifeTime;
    private float lifeTimer;
    
    private bool isHoming = false;
    private Transform homingTarget;
    private float homingDelay;
    private float homingTimer;
    private float homingTurnSpeed;
    private Vector2 initialVelocity;
    private string poolTag;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void OnEnable()
    {
        isHoming = false;
        homingTarget = null;
        homingTimer = 0;
    }

    public void Initialize(Vector2 velocity, float life, float dmg, string tag)
    {
        initialVelocity = velocity;
        rb.linearVelocity = velocity;
        lifeTime = life;
        lifeTimer = life;
        damage = dmg;
        poolTag = tag;
    }
    public void EnableHoming(Transform target, float delay, float turnSpeed)
    {
        homingTarget = target;
        homingDelay = delay;
        homingTurnSpeed = turnSpeed;
        isHoming = true;
        homingTimer = 0f;
    }

    private void FixedUpdate()
    {
        lifeTime -= Time.fixedDeltaTime;
        if(lifeTime <= 0f)
        {
            ReturnToPool();
            return;
        }

        if(acceleration != 0)
        {
            Vector2 dir = rb.linearVelocity.normalized;
            rb.linearVelocity += dir * acceleration * Time.fixedDeltaTime;
        }

        if(isHoming && homingTarget != null)
        {
            homingTimer += Time.fixedDeltaTime;
            if(homingTimer >= homingDelay)
            {
                Vector2 currentDir = rb.linearVelocity.normalized;
                Vector2 targetDir = (homingTarget.position - transform.position).normalized;

                float maxRotateRad = homingTurnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;

                Vector3 newDir3 = Vector3.RotateTowards(currentDir, targetDir, maxRotateRad, 1f);
                
                Vector2 newDir = newDir3.normalized;
                rb.linearVelocity = newDir * rb.linearVelocity.magnitude;
            }
        }

        if(rotateToVelocity && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy"))
        {
            if(other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
                ReturnToPool();
            }
        }
        else if(other.CompareTag("Environment"))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
        ObjectPooler.Instance?.ReturnToPool(gameObject, poolTag);
    }
}
