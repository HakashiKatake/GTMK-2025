using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    [Header("Player Visuals")]
    public Transform playerVisuals; // The visual representation that will flip
    private bool facingRight = true;

    [Header("Shotgun")]
    public Transform shotgun;
    public float armLength = 1.5f;
    public float rotationLerpSpeed = 10f;
    public float followDelay = 0.1f;
    private Vector2 rotationVelocity;

    [Header("Shooting")]
    public GameObject pelletPrefab;
    public Transform firePoint; // at the tip of the shotgun
    public int pelletCount = 5;
    public float spreadAngle = 10f;
    public float pelletSpeed = 20f;
    public float fireCooldown = 0.5f;
    public float recoilForce = 8f;
    public float pelletLifetime = 3f; // Time before pellets disappear
    private float lastFireTime;

    [Header("Animation")]
    public Animator animator;

    // Private variables
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float horizontalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Try to get animator from this GameObject first, then from playerVisuals
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null && playerVisuals != null)
                animator = playerVisuals.GetComponent<Animator>();
        }
        
        // If playerVisuals is not assigned, use this transform
        if (playerVisuals == null)
            playerVisuals = transform;
            
        // Error checking
        if (shotgun == null) Debug.LogError("Shotgun is not assigned!");
        if (firePoint == null) Debug.LogError("FirePoint is not assigned!");
        if (groundCheck == null) Debug.LogError("GroundCheck is not assigned!");
    }

    void Update()
    {
        HandleInput();
        CheckGroundStatus();
        HandleMovement();
        HandleJumping();
        HandleFlipping();
        UpdateShotgunPhysics();
        UpdateAnimations();
        HandleShooting();
    }

    void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Jump buffer - allows jumping slightly before hitting ground
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
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
            jumpBufferCounter = 0f; // Consume the jump buffer
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
        Vector3 scale = playerVisuals.localScale;
        scale.x *= -1;
        playerVisuals.localScale = scale;
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Set animation parameters - only using the three available parameters
            animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
            animator.SetFloat("yVelocity", rb.velocity.y);
            animator.SetBool("isJumping", !isGrounded);
            
            // No landing trigger or other parameters
        }
    }

    void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastFireTime + fireCooldown)
        {
            FireShotgun();
            lastFireTime = Time.time;
        }
    }

    void UpdateShotgunPhysics()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 toMouse = (mousePos - transform.position).normalized;
        float targetAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;

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

            // Destroy pellet after lifetime
            Destroy(pellet, pelletLifetime);
        }

        // Apply recoil in opposite direction of firing
        Vector2 recoilDir = -(shotgun.right);
        rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);
    }

    
}


