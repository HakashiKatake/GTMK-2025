using UnityEngine;

public class Spirit : MonoBehaviour
{
    [Header("Spirit Behavior")]
    public float moveSpeed = 3f;
    public float attackRange = 0.1f;  // Extremely small - spirit will collide with player
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
    private Transform _player;
    private Rigidbody2D _rb;
    private Vector3 _startPosition;
    private Vector2 _wanderDirection;
    private float _lastFireTime;
    private float _lastDirectionChangeTime;
    private float _floatTimer;
    private SpiritState _currentState = SpiritState.Wandering;

    // States
    public enum SpiritState
    {
        Wandering,
        Chasing,
        Attacking,
        Retreating
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _startPosition = transform.position;
        
        // Always try to find player when spirit starts
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
            _player = playerObject.transform;
        
        // If no player found, try alternative tags
        if (_player == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Bot");
            if (playerObject != null)
                _player = playerObject.transform;
        }

        ChangeWanderDirection();

        if (spriteRenderer != null && startTransparent)
        {
            Color color = spriteRenderer.color;
            color.a = 0.7f;
            spriteRenderer.color = color;
        }

        if (projectilePrefab == null) 
            Debug.LogWarning("Spirit: Projectile prefab not assigned!");
        if (firePoint == null) 
            firePoint = transform;
    }

    void OnEnable()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();
            
        _startPosition = transform.position;
        _currentState = SpiritState.Wandering;
        
        // Always try to find player when enabled
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
            _player = playerObject.transform;
            
        // If no player found, try alternative tags
        if (_player == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Bot");
            if (playerObject != null)
                _player = playerObject.transform;
        }
    }

    void Update()
    {
        // Always update spirit behavior when active
        CheckPlayerDistance();
        UpdateState();
        HandleMovement();
        HandleAttacking();
    }

    void CheckPlayerDistance()
    {
        // Refresh player reference if lost
        if (_player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
                _player = playerObject.transform;
            
            // Try Bot tag if Player not found
            if (_player == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Bot");
                if (playerObject != null)
                    _player = playerObject.transform;
            }
        }
        
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer > detectionRange)
        {
            // Too far - just wander
            _currentState = SpiritState.Wandering;
        }
        else if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            // Medium distance - shoot while approaching
            _currentState = SpiritState.Attacking;
        }
        else if (distanceToPlayer <= attackRange)
        {
            // Very close - chase aggressively without shooting
            _currentState = SpiritState.Chasing;
        }
    }

    void UpdateState()
    {
        switch (_currentState)
        {
            case SpiritState.Wandering:
                // Change direction periodically when no player detected
                if (Time.time - _lastDirectionChangeTime > changeDirectionTime)
                {
                    ChangeWanderDirection();
                }
                break;

            case SpiritState.Attacking:
                // Medium distance: Shoot while slowly approaching player
                if (_player != null)
                {
                    Vector2 directionToPlayer = (_player.position - transform.position).normalized;
                    // Move at moderate speed while shooting
                    _rb.velocity = directionToPlayer * (moveSpeed * 0.5f);
                }
                break;

            case SpiritState.Chasing:
                // Close distance: Chase aggressively without shooting
                if (_player != null)
                {
                    Vector2 directionToPlayer = (_player.position - transform.position).normalized;
                    // Move at good speed when chasing to collide with player
                    _rb.velocity = directionToPlayer * (moveSpeed * 0.8f);
                }
                break;
        }
    }

    void HandleMovement()
    {
        _floatTimer += Time.deltaTime;
        float floatY = Mathf.Sin(_floatTimer * floatFrequency) * floatAmplitude;

        if (_currentState == SpiritState.Wandering)
        {
            // Simple wandering - move towards a point around start position
            Vector2 targetPosition = _startPosition + (Vector3)_wanderDirection * wanderRadius;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            
            _rb.velocity = new Vector2(direction.x * moveSpeed * 0.5f, direction.y * moveSpeed * 0.5f);
            
            if (Vector2.Distance(transform.position, targetPosition) < 1f)
            {
                ChangeWanderDirection();
            }
            
            // Apply floating effect only when wandering
            Vector3 currentPos = transform.position;
            currentPos.y += floatY * Time.deltaTime * 0.3f;
            transform.position = currentPos;
        }
        // For Attacking and Chasing states, movement is handled in UpdateState()
        // No floating effect during combat to ensure precise movement towards player
    }

    void HandleAttacking()
    {
        if (_currentState != SpiritState.Attacking || _player == null) return;

        // Fire projectiles at regular intervals
        if (Time.time - _lastFireTime > 1f / fireRate)
        {
            FireProjectile();
            _lastFireTime = Time.time;
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || _player == null) return;
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx("spirit attack");
        }
        
        Vector2 direction = (_player.position - firePoint.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        if (!projectile.TryGetComponent<Rigidbody2D>(out var projRb))
        {
            projRb = projectile.AddComponent<Rigidbody2D>();
            projRb.gravityScale = 0f;
        }
        projRb.velocity = direction * projectileSpeed;
        
        if (!projectile.TryGetComponent<Collider2D>(out var _))
        {
            var circleCollider = projectile.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
            circleCollider.radius = 0.1f;
        }
       
        var projLogic = projectile.AddComponent<SpiritProjectileLogic>();
        projLogic.Initialize(direction, projectileSpeed, projectileLifetime, projectileDamage, playerTag);
    }

    void ChangeWanderDirection()
    {
        _wanderDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        _lastDirectionChangeTime = Time.time;
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
            Gizmos.DrawWireSphere(_startPosition, wanderRadius);
        else
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }

    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        _player = newTarget;
    }

    public void ForceState(SpiritState newState)
    {
        _currentState = newState;
    }

    public SpiritState GetCurrentState()
    {
        return _currentState;
    }
}

// Simple projectile logic class to handle spirit projectile behavior
public class SpiritProjectileLogic : MonoBehaviour
{
    private Vector2 _direction;
    private float _damage;
    private string _targetTag;
    private bool _hasHit;

    public void Initialize(Vector2 moveDirection, float moveSpeed, float projectileLifetime, float projectileDamage, string playerTag)
    {
        _direction = moveDirection.normalized;
        _damage = projectileDamage;
        _targetTag = playerTag;

        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = _direction * moveSpeed;
        }

        Destroy(gameObject, projectileLifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;

        if (other.CompareTag(_targetTag))
        {
            _hasHit = true;
            if (other.TryGetComponent<PlayerController2D>(out var player))
            {
                DamagePlayer(player);
            }
            else if (other.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage(_damage);
            }
            DestroyProjectile();
        }
        else if (!other.CompareTag("Spirit") && other.gameObject.layer != gameObject.layer)
        {
            _hasHit = true;
            DestroyProjectile();
        }
    }

    void DamagePlayer(PlayerController2D player)
    {
        if (player.TryGetComponent<Rigidbody2D>(out var playerRb))
        {
            playerRb.AddForce(_direction * 5f, ForceMode2D.Impulse);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx("human hurt");
        }
    }

    void DestroyProjectile()
    {
        if (TryGetComponent<TrailRenderer>(out var trail))
        {
            trail.enabled = false;
        }
        if (TryGetComponent<ParticleSystem>(out var particles))
        {
            particles.Stop();
        }
        Destroy(gameObject);
    }
}
