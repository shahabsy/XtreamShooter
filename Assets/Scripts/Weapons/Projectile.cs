using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        gameObject.SetActive(false);
        CancelInvoke();
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;
    public void SetPoolTag(string tag) => poolTag = tag;
}
