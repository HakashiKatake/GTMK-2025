using UnityEngine;

public class Bot : MonoBehaviour
{
    [Header("Bot Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    [Header("Bot Visuals")]
    public Transform botVisuals; // The visual representation that will flip
    private bool facingRight = true;

    [Header("Shotgun")]
    public Transform shotgun;
    public float armLength = 1.5f;
    public float followDelay = 0.1f;
    private Vector2 rotationVelocity;

    [Header("Shooting")]
    public GameObject pelletPrefab;
    public Transform firePoint;
    public int pelletCount = 5;
    public float spreadAngle = 10f;
    public float pelletSpeed = 20f;
    public float fireCooldown = 0.5f;
    public float recoilForce = 8f;
    public float pelletLifetime = 3f;
    private float lastFireTime;

    [Header("Bot AI")]
    public float detectionRange = 10f;
    public float attackRange = 8f;
    public float wanderRadius = 5f;
    public float changeDirectionTime = 3f;
    public LayerMask enemyLayers = 1;
    public string enemyTag = "Spirit";

    [Header("Animation")]
    public Animator animator;

    // Private variables - Movement
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float horizontalInput;

    // Private variables - AI
    private Transform target;
    private Vector3 startPosition;
    private Vector2 wanderDirection;
    private float lastDirectionChangeTime;
    private float lastJumpTime;
    private bool isAttacking = false;

    // Bot States
    public enum BotState
    {
        Wandering,
        Chasing,
        Attacking,
        Fleeing
    }

    private BotState currentState = BotState.Wandering;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        
        // Try to get animator from this GameObject first, then from botVisuals
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null && botVisuals != null)
                animator = botVisuals.GetComponent<Animator>();
        }
        
        // If botVisuals is not assigned, use this transform
        if (botVisuals == null)
            botVisuals = transform;
            
        // Error checking
        if (shotgun == null) Debug.LogError("Bot: Shotgun is not assigned!");
        if (firePoint == null) Debug.LogError("Bot: FirePoint is not assigned!");
        if (groundCheck == null) Debug.LogError("Bot: GroundCheck is not assigned!");

        // Setup initial wander direction
        ChangeWanderDirection();
    }

    void Update()
    {
        CheckGroundStatus();
        UpdateAI();
        HandleMovement();
        HandleJumping();
        HandleFlipping();
        UpdateShotgunPhysics();
        UpdateAnimations();
        HandleShooting();
    }

    void UpdateAI()
    {
        FindNearestEnemy();
        UpdateBotState();
        DetermineBotInput();
    }

    void FindNearestEnemy()
    {
        // Find all enemies in detection range
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayers);
        
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag(enemyTag) && enemy.gameObject != gameObject)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
        }

        target = closestEnemy;
    }

    void UpdateBotState()
    {
        if (target == null)
        {
            currentState = BotState.Wandering;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // State transitions
        if (distanceToTarget <= attackRange)
        {
            currentState = BotState.Attacking;
        }
        else if (distanceToTarget <= detectionRange)
        {
            currentState = BotState.Chasing;
        }
        else
        {
            currentState = BotState.Wandering;
        }
    }

    void DetermineBotInput()
    {
        horizontalInput = 0f;
        jumpBufferCounter -= Time.deltaTime;

        switch (currentState)
        {
            case BotState.Wandering:
                HandleWandering();
                break;
            case BotState.Chasing:
                HandleChasing();
                break;
            case BotState.Attacking:
                HandleAttacking();
                break;
        }
    }

    void HandleWandering()
    {
        // Change direction periodically
        if (Time.time - lastDirectionChangeTime > changeDirectionTime)
        {
            ChangeWanderDirection();
        }

        // Move towards wander target
        Vector2 wanderTarget = startPosition + (Vector3)wanderDirection * wanderRadius;
        float directionToTarget = wanderTarget.x - transform.position.x;
        
        if (Mathf.Abs(directionToTarget) > 0.5f)
        {
            horizontalInput = Mathf.Sign(directionToTarget);
        }

        // Random jumping while wandering
        if (Random.Range(0f, 1f) < 0.002f && isGrounded)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    void HandleChasing()
    {
        if (target == null) return;

        // Move towards target
        float directionToTarget = target.position.x - transform.position.x;
        horizontalInput = Mathf.Sign(directionToTarget);

        // Jump if there's an obstacle or target is higher
        bool shouldJump = false;
        
        // Check if target is higher
        if (target.position.y > transform.position.y + 1f)
        {
            shouldJump = true;
        }

        // Check for obstacles ahead
        Vector2 rayOrigin = transform.position + Vector3.right * horizontalInput * 1f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 2f, groundLayer);
        if (hit.collider == null) // No ground ahead, might need to jump
        {
            shouldJump = true;
        }

        if (shouldJump && isGrounded && Time.time - lastJumpTime > 1f)
        {
            jumpBufferCounter = jumpBufferTime;
            lastJumpTime = Time.time;
        }
    }

    void HandleAttacking()
    {
        if (target == null) return;

        // Move slightly towards target but focus on aiming
        float directionToTarget = target.position.x - transform.position.x;
        horizontalInput = Mathf.Sign(directionToTarget) * 0.3f; // Slower movement while attacking

        // Set attacking flag for shooting
        isAttacking = true;
    }

    void ChangeWanderDirection()
    {
        wanderDirection = new Vector2(Random.Range(-1f, 1f), 0f).normalized;
        lastDirectionChangeTime = Time.time;
    }

    void CheckGroundStatus()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Coyote time - allows jumping slightly after leaving ground
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;
    }

    void HandleMovement()
    {
        // Apply horizontal movement
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    void HandleJumping()
    {
        // Jump if we have jump buffer and are grounded (or in coyote time)
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;
        }
    }

    void HandleFlipping()
    {
        // Flip based on movement input
        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = botVisuals.localScale;
        scale.x *= -1;
        botVisuals.localScale = scale;
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Set animation parameters - only using the three available parameters
            animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
            animator.SetFloat("yVelocity", rb.velocity.y);
            animator.SetBool("isJumping", !isGrounded);
        }
    }

    void HandleShooting()
    {
        if (currentState == BotState.Attacking && target != null && Time.time >= lastFireTime + fireCooldown)
        {
            FireShotgun();
            lastFireTime = Time.time;
        }
        
        // Reset attacking flag
        if (currentState != BotState.Attacking)
        {
            isAttacking = false;
        }
    }

    void UpdateShotgunPhysics()
    {
        Vector3 targetPos;
        
        if (target != null && isAttacking)
        {
            // Aim at target
            targetPos = target.position;
        }
        else
        {
            // Default forward aiming
            targetPos = transform.position + (facingRight ? Vector3.right : Vector3.left) * 5f;
        }

        Vector2 toTarget = (targetPos - transform.position).normalized;
        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

        float currentAngle = shotgun.eulerAngles.z;
        float smoothedAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref rotationVelocity.x, followDelay);

        shotgun.rotation = Quaternion.Euler(0, 0, smoothedAngle);
        shotgun.position = transform.position + shotgun.right * armLength;
    }

    void FireShotgun()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            float angleOffset = Random.Range(-spreadAngle, spreadAngle);
            float baseAngle = shotgun.eulerAngles.z + angleOffset;
            Quaternion pelletRot = Quaternion.Euler(0, 0, baseAngle);

            GameObject pellet = Instantiate(pelletPrefab, firePoint.position, pelletRot);
            Rigidbody2D pelletRb = pellet.GetComponent<Rigidbody2D>();
            
            if (pelletRb != null)
            {
                pelletRb.velocity = pellet.transform.right * pelletSpeed;
            }

            // Add collision detection to pellet
            PelletCollision pelletScript = pellet.GetComponent<PelletCollision>();
            if (pelletScript == null)
            {
                pelletScript = pellet.AddComponent<PelletCollision>();
            }

            // Destroy pellet after lifetime
            Destroy(pellet, pelletLifetime);
        }

        // Apply recoil in opposite direction of firing
        Vector2 recoilDir = -(shotgun.right);
        rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Play hit sound when getting hit
        if (other.CompareTag("Spirit") || other.CompareTag("SpiritProjectile"))
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySfx("human hurt");
            }
        }
    }

    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void ForceState(BotState newState)
    {
        currentState = newState;
    }

    public BotState GetCurrentState()
    {
        return currentState;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    // Helper method to visualize bot's detection and attack ranges
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

        // Draw ground check
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Draw line to target
        if (target != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
