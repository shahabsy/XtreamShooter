using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance {  get; private set; }

    [Header("MovementSettings")]
    [SerializeField] private float moveSpeed = 8f;
    private float leftBoundary, rightBoundary, topBoundary, bottomBoundary;

    [Header("Auto Bounds (Camera)")]
    [SerializeField] private Vector2 screenPadding = new Vector2(0.5f, 0.5f); // world units padding from edges

    // Use collider for extents if present, otherwise fall back to sprite renderer
    [Header("Extent Calculation")]
    [SerializeField] private bool useColliderForExtents = true;
    [SerializeField] private bool previewBounds = false;

    [Header("Shooting Settings")]
    //[SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private float nextFireTime;

    private event Action<string> OnTriggerEvent;

    //private Camera mainCamera;
    private float halfWidth = 0.5f;
    private float halfHeight = 0.5f;

    // runtime computed bounds
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Attack.performed -= OnAttack;
    }

    void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        HandleMovement();
        
        bool isAttacking = inputActions.Player.Attack.IsPressed();
        if (isAttacking && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void LateUpdate()
    {
        UpdateBoundsFromCamera();
        ClampPlayerPosition();
    }

    private void HandleMovement()
    {
        Vector2 movement = moveInput * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
        if (moveInput.magnitude > 0.1) FireTrigger("Movement");
    }

    private void UpdateBoundsFromCamera()
    {
        if (Camera.main == null) return;

        // Use distance from camera to player for proper viewport 
        float zDistance = Mathf.Abs(Camera.main.transform.position.x - transform.position.z);
        Vector3 leftBottom = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 rightTop = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

        // Apply padding and account for player sprite/collider extents
        leftBoundary = leftBottom.x + halfWidth + screenPadding.x;
        rightBoundary = rightTop.x - halfWidth - screenPadding.x;
        bottomBoundary = leftBottom.y + halfHeight + screenPadding.y;
        topBoundary = rightTop.y - halfHeight - screenPadding.y;
    }

    private void ClampPlayerPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftBoundary, rightBoundary);
        pos.y = Mathf.Clamp(pos.y, bottomBoundary, topBoundary);
        transform.position = pos;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Shoot();
            FireTrigger("Weapon1");
        }
    }

    private void Shoot()
    {
        if (ObjectPooler.Instance != null && firePoint != null)
        {
            ObjectPooler.Instance.SpawnFromPool("PlayerBullet", firePoint.position, Quaternion.identity);
        }
    }

    public void RecalculateExtents()
    {
        if(useColliderForExtents)
        {
            Collider col2d = GetComponent<Collider>();
            if(col2d != null)
            {
                var size = col2d.bounds.size;
                halfWidth = size.x * 0.5f;
                halfHeight = size.y * 0.5f;
                return;
            }
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var size = sr.bounds.size;
            halfWidth = size.x * 0.5f;
            halfHeight = size.y * 0.5f;
            return;
        }
        // default
        halfWidth = 0.5f;
        halfHeight = 0.5f;
    }

    public void RegisterTriggerListener(Action<string> listener)
    {
        OnTriggerEvent += listener;
    }
    public void UnregisterTriggerListener(Action<string> listener)
    {
        OnTriggerEvent -= listener;
    }

    public void UnregisterAllListeners(string trigger)
    {
        OnTriggerEvent = null;
    }

    private void FireTrigger(string triggerName)
    {
        OnTriggerEvent?.Invoke(triggerName);
    }

    // Draw preview of computed bounds in Scene view when previewBounds is enabled (or when selected)
    private void OnDrawGizmos()
    {
        if (!previewBounds) return;
        if (Camera.main == null) return;

        float zDistance = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 leftBottom = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 rightTop = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

        float l = leftBottom.x + halfWidth + screenPadding.x;
        float r = rightTop.x - halfWidth - screenPadding.x;
        float b = leftBottom.y + halfHeight + screenPadding.y;
        float t = rightTop.y - halfHeight - screenPadding.y;

        Vector3 center = new Vector3((l + r) * 0.5f, (b + t) * 0.5f, transform.position.z);
        Vector3 size = new Vector3(Mathf.Abs(r - l), Mathf.Abs(t - b), 0.01f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);

        // Also draw player extents at its current position
        Gizmos.color = Color.yellow;
        Vector3 playerCenter = transform.position;
        Vector3 playerSize = new Vector3(halfWidth * 2f, halfHeight * 2f, 0.01f);
        Gizmos.DrawWireCube(playerCenter, playerSize);
    }
}
