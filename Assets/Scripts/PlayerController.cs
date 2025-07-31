using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

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
    private float lastFireTime;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (shotgun == null) Debug.LogError("Shotgun is not assigned!");
        if (firePoint == null) Debug.LogError("FirePoint is not assigned!");
    }

    void Update()
    {
        HandleMovement();
        UpdateShotgunPhysics();

        if (Input.GetMouseButtonDown(0) && Time.time >= lastFireTime + fireCooldown)
        {
            FireShotgun();
            lastFireTime = Time.time;
        }
    }

    void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
            pelletRb.velocity = pellet.transform.right * pelletSpeed;
        }

        // Apply recoil in opposite direction of firing
        Vector2 recoilDir = -(shotgun.right);
        rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);
    }
}
