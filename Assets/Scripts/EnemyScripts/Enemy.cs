using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float leftBoundary = -10f;

    private float nextFireTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (firePoint == null) firePoint = transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        if (transform.position.x < leftBoundary)
        {
            Destroy(gameObject);
        }

        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (enemyBulletPrefab != null)
        {
            ObjectPooler.Instance.SpawnFromPool("EnemyBullet", firePoint.position, Quaternion.identity);
        }
    }
}
