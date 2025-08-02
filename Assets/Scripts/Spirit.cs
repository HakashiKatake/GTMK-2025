using UnityEngine;

public class Spirit : MonoBehaviour
{
    [Header("Spirit Behavior")]
    public float moveSpeed = 3f;
    public float attackRange = 8f;
    public float detectionRange = 12f;
    public LayerMask playerLayer = 1;

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float projectileSpeed = 6f;
    public float projectileLifetime = 5f;

    [Header("Movement Patterns")]
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 1f;
    public float wanderRadius = 5f;
    public float changeDirectionTime = 3f;

    [Header("Visual Effects")]
    public SpriteRenderer spriteRenderer;
    public float fadeInOutSpeed = 2f;
    public bool startTransparent = true;

    // Private variables
    private Transform player;
    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Vector2 wanderDirection;
    private float lastFireTime;
    private float lastDirectionChangeTime;
    private float floatTimer;
    private bool isPlayerInRange = false;

    // States
    public enum SpiritState
    {
        Wandering,
        Chasing,
        Attacking,
        Retreating
    }

    private SpiritState currentState = SpiritState.Wandering;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        
        // Find player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;

        // Setup initial wander direction
        ChangeWanderDirection();

        // Setup sprite transparency if needed
        if (spriteRenderer != null && startTransparent)
        {
            Color color = spriteRenderer.color;
            color.a = 0.7f;
            spriteRenderer.color = color;
        }

        // Error checking
        if (projectilePrefab == null) 
            Debug.LogWarning("Spirit: Projectile prefab not assigned!");
        if (firePoint == null) 
            firePoint = transform; // Use spirit position as fallback
    }

    void Update()
    {
        CheckPlayerDistance();
        UpdateState();
        HandleMovement();
        HandleAttacking();
        UpdateVisuals();
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Update player in range status
        isPlayerInRange = distanceToPlayer <= detectionRange;

        // State transitions based on distance
        if (distanceToPlayer <= attackRange && currentState != SpiritState.Attacking)
        {
            currentState = SpiritState.Attacking;
        }
        else if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange && currentState == SpiritState.Wandering)
        {
            currentState = SpiritState.Chasing;
        }
        else if (distanceToPlayer > detectionRange && currentState != SpiritState.Wandering)
        {
            currentState = SpiritState.Wandering;
        }
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case SpiritState.Wandering:
                // Change direction periodically
                if (Time.time - lastDirectionChangeTime > changeDirectionTime)
                {
                    ChangeWanderDirection();
                }
                break;

            case SpiritState.Chasing:
                // Move towards player
                if (player != null)
                {
                    Vector2 directionToPlayer = (player.position - transform.position).normalized;
                    rb.velocity = directionToPlayer * moveSpeed;
                }
                break;

            case SpiritState.Attacking:
                // Slow down when attacking
                rb.velocity = rb.velocity * 0.5f;
                break;
        }
    }

    void HandleMovement()
    {
        // Add floating motion
        floatTimer += Time.deltaTime;
        float floatY = Mathf.Sin(floatTimer * floatFrequency) * floatAmplitude;

        if (currentState == SpiritState.Wandering)
        {
            // Wander around starting position
            Vector2 targetPosition = startPosition + (Vector3)wanderDirection * wanderRadius;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            
            rb.velocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);
            
            // Check if we're close to target, then change direction
            if (Vector2.Distance(transform.position, targetPosition) < 1f)
            {
                ChangeWanderDirection();
            }
        }

        // Apply floating effect
        Vector3 currentPos = transform.position;
        currentPos.y = startPosition.y + floatY;
        transform.position = currentPos;
    }

    void HandleAttacking()
    {
        if (currentState != SpiritState.Attacking || player == null) return;

        // Fire projectiles at regular intervals
        if (Time.time - lastFireTime > 1f / fireRate)
        {
            FireProjectile();
            lastFireTime = Time.time;
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        // Calculate direction to player
        Vector2 direction = (player.position - firePoint.position).normalized;
        
        // Instantiate projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // Set up projectile
        SpiritProjectile projScript = projectile.GetComponent<SpiritProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(direction, projectileSpeed, projectileLifetime);
        }
        else
        {
            // Fallback: just add velocity to rigidbody
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.velocity = direction * projectileSpeed;
                Destroy(projectile, projectileLifetime);
            }
        }

        // Add some visual/audio feedback here if needed
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("SpiritAttack");
        }
    }

    void UpdateVisuals()
    {
        // Fade sprite based on distance to player
        if (spriteRenderer != null && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            float maxDistance = detectionRange * 1.5f;
            float alpha = Mathf.Lerp(0.3f, 1f, 1f - (distanceToPlayer / maxDistance));
            
            Color color = spriteRenderer.color;
            color.a = Mathf.Clamp(alpha, 0.3f, 1f);
            spriteRenderer.color = color;
        }

        // Face the player when chasing or attacking
        if ((currentState == SpiritState.Chasing || currentState == SpiritState.Attacking) && player != null)
        {
            Vector2 direction = player.position - transform.position;
            if (direction.x > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void ChangeWanderDirection()
    {
        wanderDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        lastDirectionChangeTime = Time.time;
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw wander radius
        Gizmos.color = Color.blue;
        if (Application.isPlaying)
            Gizmos.DrawWireSphere(startPosition, wanderRadius);
        else
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }

    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
    }

    public void ForceState(SpiritState newState)
    {
        currentState = newState;
    }

    public SpiritState GetCurrentState()
    {
        return currentState;
    }
}
