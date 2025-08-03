using UnityEngine;
using UnityEngine.SceneManagement;

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
    private bool _facingRight = true;

    [Header("Shotgun")]
    public Transform shotgun;
    public float armLength = 1.5f;
    public float rotationLerpSpeed = 10f;
    public float followDelay = 0.1f;
    private Vector2 _rotationVelocity;

    [Header("Shooting")]
    public GameObject pelletPrefab;
    public Transform firePoint; // at the tip of the shotgun
    public int pelletCount = 5;
    public float spreadAngle = 10f;
    public float pelletSpeed = 20f;
    public float fireCooldown = 0.5f;
    public float recoilForce = 8f;
    public float pelletLifetime = 3f; // Time before pellets disappear
    private float _lastFireTime;

    [Header("Animation")]
    public Animator animator;

    // Private variables
    private Rigidbody2D _rb;
    private bool _isGrounded;
    private bool _wasGrounded;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private float _horizontalInput;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        
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
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Jump buffer - allows jumping slightly before hitting ground
        if (Input.GetButtonDown("Jump"))
            _jumpBufferCounter = jumpBufferTime;
        else
            _jumpBufferCounter -= Time.deltaTime;
    }

    void CheckGroundStatus()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Coyote time - allows jumping slightly after leaving ground
        if (_isGrounded)
            _coyoteTimeCounter = coyoteTime;
        else
            _coyoteTimeCounter -= Time.deltaTime;
    }

    void HandleMovement()
    {
        // Apply horizontal movement
        _rb.velocity = new Vector2(_horizontalInput * moveSpeed, _rb.velocity.y);
    }

    void HandleJumping()
    {
        // Jump if we have jump buffer and are grounded (or in coyote time)
        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            _jumpBufferCounter = 0f; // Consume the jump buffer
        }
    }

    void HandleFlipping()
    {
        // Flip based on movement input
        if (_horizontalInput > 0 && _facingRight)
        {
            Flip();
        }
        else if (_horizontalInput < 0 && !_facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = playerVisuals.localScale;
        scale.x *= -1;
        playerVisuals.localScale = scale;
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Set animation parameters - only using the three available parameters
            animator.SetFloat("xVelocity", Mathf.Abs(_rb.velocity.x));
            animator.SetFloat("yVelocity", _rb.velocity.y);
            animator.SetBool("isJumping", !_isGrounded);
            
            // No landing trigger or other parameters
        }
    }

    void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= _lastFireTime + fireCooldown)
        {
            FireShotgun();
            _lastFireTime = Time.time;
        }
    }

    void UpdateShotgunPhysics()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 toMouse = (mousePos - transform.position).normalized;
        float targetAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;

        float currentAngle = shotgun.eulerAngles.z;
        float smoothedAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref _rotationVelocity.x, followDelay);

        shotgun.rotation = Quaternion.Euler(0, 0, smoothedAngle);
        shotgun.position = transform.position + shotgun.right * armLength;
    }

    void FireShotgun()
    {
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySfx("shotgun");
        }
        
        for (int i = 0; i < pelletCount; i++)
        {
            float angleOffset = Random.Range(-spreadAngle, spreadAngle);
            float baseAngle = shotgun.eulerAngles.z + angleOffset;
            Quaternion pelletRot = Quaternion.Euler(0, 0, baseAngle);

            GameObject pellet = Instantiate(pelletPrefab, firePoint.position, pelletRot);
            Rigidbody2D pelletRb = pellet.GetComponent<Rigidbody2D>();
            
            if (pelletRb)
            {
                pelletRb.velocity = pellet.transform.right * pelletSpeed;
            }

            // Add collision detection to pellet
            PelletCollision pelletScript = pellet.GetComponent<PelletCollision>();
            if (!pelletScript)
            {
                pelletScript = pellet.AddComponent<PelletCollision>();
            }

            Destroy(pellet, pelletLifetime);
        }

        Vector2 recoilDir = -(shotgun.right);
        _rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check for water layer - restart scene if player touches water
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            RestartScene();
            return;
        }
        
        if (!(other.CompareTag("Spirit") || other.CompareTag("Projectile")))return;
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySfx("human hurt");
        }
        
        // Take damage from spirits or projectiles
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(1f);
        }
        
        if (other.gameObject.CompareTag("Spirit"))GameManager.instance.SwitchToUndead();
    }

    void RestartScene()
    {
        // Play drowning sound if available
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySfx("human drown");
        }
        
        // Restart the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
