using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("MovementSettings")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Auto Bounds (Camera)")]
    [SerializeField] private Vector2 screenPadding = new Vector2(0.5f, 0.5f); // world units padding from edges

    // Use collider for extents if present, otherwise fall back to sprite renderer
    [Header("Extent Calculation")]
    [SerializeField] private bool useColliderForExtents = true;
    [SerializeField] private bool previewBounds = false;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private float nextFireTime;

    private Camera mainCamera;
    private float halfWidth = 0.5f;
    private float halfHeight = 0.5f;

    // runtime computed bounds
    private float leftBoundary, rightBoundary, topBoundary, bottomBoundary;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        mainCamera = Camera.main;
        RecalculateExtents();

    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
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
    }

    private void UpdateBoundsFromCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        // Use distance from camera to player for proper viewport 
        float zDistance = Mathf.Abs(mainCamera.transform.position.x - transform.position.z);
        Vector3 leftBottom = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 rightTop = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

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

    // Draw preview of computed bounds in Scene view when previewBounds is enabled (or when selected)
    private void OnDrawGizmos()
    {
        if (!previewBounds) return;

        Camera cam = mainCamera;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        float zDistance = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 leftBottom = cam.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 rightTop = cam.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

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
