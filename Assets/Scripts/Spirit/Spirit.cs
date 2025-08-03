using UnityEngine;

public class Spirit : MonoBehaviour
{
    [Header("Spirit Behavior")]
    public float moveSpeed = 3f;
    public float attackRange = 2f;  // Much smaller - spirit gets very close before chasing
    public float detectionRange = 12f;
    public string playerTag = "Player";

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float projectileSpeed = 6f;
    public float projectileLifetime = 5f;
    public float projectileDamage = 1f;

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
    private Vector3 originalScale; // Store original scale from inspector

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
        
        // Store the original scale set in the inspector
        originalScale = transform.localScale;
        
        // Find player
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
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
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Update player in range status
        isPlayerInRange = distanceToPlayer <= detectionRange;

        // New behavior: 
        // - Far away (outside detection range): Wander
        // - Medium distance (inside detection but outside attack range): Shoot and approach
        // - Very close (inside attack range): Chase aggressively without shooting
        
        if (distanceToPlayer > detectionRange)
        {
            // Too far - just wander
            currentState = SpiritState.Wandering;
        }
        else if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            // Medium distance - shoot while approaching
            currentState = SpiritState.Attacking;
        }
        else if (distanceToPlayer <= attackRange)
        {
            // Very close - chase aggressively without shooting
            currentState = SpiritState.Chasing;
        }
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case SpiritState.Wandering:
                // Change direction periodically when no player detected
                if (Time.time - lastDirectionChangeTime > changeDirectionTime)
                {
                    ChangeWanderDirection();
                }
                break;

            case SpiritState.Attacking:
                // Medium distance: Shoot while slowly approaching player
                if (player != null)
                {
                    Vector2 directionToPlayer = (player.position - transform.position).normalized;
                    // Move slower while shooting to maintain distance and accuracy
                    rb.velocity = directionToPlayer * (moveSpeed * 0.6f);
                }
                break;

            case SpiritState.Chasing:
                // Close distance: Chase aggressively without shooting
                if (player != null)
                {
                    Vector2 directionToPlayer = (player.position - transform.position).normalized;
                    // Move faster when chasing up close
                    rb.velocity = directionToPlayer * (moveSpeed * 1.2f);
                }
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
        
        // Set rotation to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // Add rigidbody and set velocity
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb == null)
        {
            projRb = projectile.AddComponent<Rigidbody2D>();
            projRb.gravityScale = 0f; 
        }
        projRb.velocity = direction * projectileSpeed;
        
        // Add collider if it doesn't exist
        Collider2D projCollider = projectile.GetComponent<Collider2D>();
        if (projCollider == null)
        {
            CircleCollider2D circleCollider = projectile.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
            circleCollider.radius = 0.1f;
        }
       
        SpiritProjectileLogic projLogic = projectile.AddComponent<SpiritProjectileLogic>();
        projLogic.Initialize(direction, projectileSpeed, projectileLifetime, projectileDamage, playerTag);

        // Add some visual/audio feedback here if needed
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("SpiritAttack");
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

// Simple projectile logic class to handle spirit projectile behavior
public class SpiritProjectileLogic : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifetime;
    private float damage;
    private string targetTag;
    private float spawnTime;
    private bool hasHit = false;

    public void Initialize(Vector2 moveDirection, float moveSpeed, float projectileLifetime, float projectileDamage, string playerTag)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        lifetime = projectileLifetime;
        damage = projectileDamage;
        targetTag = playerTag;
        spawnTime = Time.time;

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Fade out near end of lifetime
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            float timeRemaining = lifetime - (Time.time - spawnTime);
            if (timeRemaining < 1f)
            {
                Color color = spriteRenderer.color;
                color.a = timeRemaining;
                spriteRenderer.color = color;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Check if hit player using tag
        if (other.CompareTag(targetTag))
        {
            // Try to damage player
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                DamagePlayer(player);
            }

            // Alternative: Try generic health component
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            hasHit = true;
            CreateHitEffect();
            DestroyProjectile();
        }
        // Hit solid objects (walls, terrain, etc.) - avoid hitting spirits
        else if (!other.CompareTag("Spirit") && other.gameObject.layer != gameObject.layer)
        {
            hasHit = true;
            CreateHitEffect();
            DestroyProjectile();
        }
    }

    void DamagePlayer(PlayerController2D player)
    {
        // Apply knockback
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockbackDirection = direction;
            float knockbackForce = 5f;
            playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        // Play hit sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("human hurt");
        }

        Debug.Log("Player hit by spirit projectile!");
    }

    void CreateHitEffect()
    {
        // Play hit sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ProjectileHit");
        }
    }

    void DestroyProjectile()
    {
        // Stop any trail effects
        TrailRenderer trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        // Stop particles
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles != null)
        {
            particles.Stop();
        }

        // Destroy the game object
        Destroy(gameObject);
    }
}
