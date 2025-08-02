using UnityEngine;
using UnityEngine.UI;

public class SpiritPlayer : MonoBehaviour
{
    [Header("Spirit Movement")]
    public float moveSpeed = 5f;
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 1f;

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    public float projectileLifetime = 5f;
    public float projectileDamage = 1f;
    public float fireCooldown = 0.3f;

    [Header("Possession")]
    public float possessionRange = 3f;
    public LayerMask possessionTargets = 1;
    public string targetTag = "Player";

    [Header("Visual Effects")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color possessionColor = Color.cyan;

    [Header("UI")]
    public Text possessionUI;
    public string possessionText = "Press E to Possess";

    // Private variables
    private Rigidbody2D rb;
    private Vector3 startPosition;
    private float floatTimer;
    private float lastFireTime;
    private bool facingRight = true;
    private GameObject nearbyTarget;
    private bool canPossess = false;

    // Input
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;

        // Setup rigidbody for spirit movement
        if (rb != null)
        {
            rb.gravityScale = 0f; // Spirits don't fall
        }

        // Error checking
        if (projectilePrefab == null)
            Debug.LogWarning("SpiritPlayer: Projectile prefab not assigned!");
        if (firePoint == null)
            firePoint = transform; // Use spirit position as fallback
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Setup UI
        if (possessionUI != null)
            possessionUI.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleShooting();
        HandlePossession();
        UpdateVisuals();
        CheckNearbyTargets();
    }

    void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void HandleMovement()
    {
        // Spirit can move in all directions (including up/down)
        Vector2 movement = new Vector2(horizontalInput, verticalInput).normalized;
        rb.velocity = movement * moveSpeed;

        // Add floating motion
        floatTimer += Time.deltaTime;
        float floatY = Mathf.Sin(floatTimer * floatFrequency) * floatAmplitude;
        
        // Apply floating effect on top of movement
        Vector3 currentPos = transform.position;
        currentPos.y += floatY * Time.deltaTime;
        transform.position = currentPos;

        // Handle flipping
        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            Flip();
        }
    }

    void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastFireTime + fireCooldown)
        {
            FireProjectile();
            lastFireTime = Time.time;
        }
    }

    void HandlePossession()
    {
        if (Input.GetKeyDown(KeyCode.E) && canPossess && nearbyTarget != null)
        {
            PossessTarget(nearbyTarget);
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null) return;

        // Get mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // Calculate direction to mouse
        Vector2 direction = (mousePos - firePoint.position).normalized;
        
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
            projRb.gravityScale = 0f; // No gravity for spirit projectiles
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
        
        // Add projectile component for hit detection
        SpiritProjectileLogic projLogic = projectile.AddComponent<SpiritProjectileLogic>();
        projLogic.Initialize(direction, projectileSpeed, projectileLifetime, projectileDamage, targetTag);

        // Play attack sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("SpiritAttack");
        }
    }

    void CheckNearbyTargets()
    {
        
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, possessionRange, possessionTargets);
        
        GameObject closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D target in targets)
        {
            
            if (target.CompareTag(targetTag) && target.gameObject != gameObject)
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target.gameObject;
                }
            }
        }

        // Update possession state
        if (closestTarget != null)
        {
            nearbyTarget = closestTarget;
            canPossess = true;
            ShowPossessionUI(true);
        }
        else
        {
            nearbyTarget = null;
            canPossess = false;
            ShowPossessionUI(false);
        }
    }

    void PossessTarget(GameObject target)
    {
        Debug.Log($"Possessing {target.name}!");

        // Play possession sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Possession");
        }

        // Notify game state manager about possession
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.TryPossession(target);
        }

        // You can add possession effects here
        // For example, disable this spirit and enable the possessed target as player
    }

    void ShowPossessionUI(bool show)
    {
        if (possessionUI != null)
        {
            possessionUI.gameObject.SetActive(show);
            if (show)
            {
                possessionUI.text = possessionText;
            }
        }
    }

    void UpdateVisuals()
    {
        // Change color when near possession target
        if (spriteRenderer != null)
        {
            if (canPossess)
            {
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, possessionColor, Time.deltaTime * 5f);
            }
            else
            {
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, normalColor, Time.deltaTime * 5f);
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Public methods for external control
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        if (active)
        {
            // Reset position and state when becoming active
            rb.velocity = Vector2.zero;
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        startPosition = position;
    }

    public bool IsNearPossessionTarget()
    {
        return canPossess;
    }

    public GameObject GetNearbyTarget()
    {
        return nearbyTarget;
    }

    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        // Draw possession range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, possessionRange);
    }

    void OnDrawGizmos()
    {
        // Draw possession range when nearby target
        if (canPossess)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, possessionRange);
        }
    }
}
