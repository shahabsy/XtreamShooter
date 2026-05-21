using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Advanced")]
    public float acceleration = 0f;
    public bool rotateToVelocity = true;
    public float damage;

    private Rigidbody2D rb;
    private float lifeTime;
    private float lifeTimer;
    
    private bool isHoming = false;
    private Transform homingTarget;
    private float homingDelay;
    private float homingTimer;
    private float homingTurnSpeed;
    private Vector2 initialiVelocity;
    private string poolTag;
    private Enemy owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }
    private void OnEnable()
    {
        isHoming = false;
        homingTarget = null;
        homingTimer = 0f;
    }
    public void Initialize(Vector2 velocity, float life, float dmg, string tag, Enemy ownerEnemy)
    {
        initialiVelocity = velocity;
        rb.linearVelocity = velocity;
        lifeTime = life;
        lifeTimer = life;
        damage = dmg;
        poolTag = tag;
        owner = ownerEnemy;
        isHoming = false;
        homingTimer = 0f;
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
        lifeTimer -= Time.fixedDeltaTime;
        if(lifeTimer <= 0)
        {
            //Debug.Log($"{name} retuned to pool after {lifeTime} secs.");
            ReturnToPool();
            return;
        }

        if(acceleration != 0f)
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

                float maxRotate = homingTurnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
                Vector2 newDir = Vector3.RotateTowards(currentDir, targetDir, maxRotate, 1f).normalized;
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
        if (other.GetComponent<Enemy>() == owner) return;
        //Debug.Log($"{other.name}");
        IDamageable damageable = other.GetComponent<IDamageable>();
        if(damageable != null)
        {
            damageable.TakeDamage(damage);
            ReturnToPool();
        }
        else if(!other.CompareTag("Player"))
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
        ObjectPooler.Instance?.ReturnToPool(gameObject, poolTag);
    }
}
