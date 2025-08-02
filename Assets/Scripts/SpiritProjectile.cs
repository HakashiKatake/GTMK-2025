using UnityEngine;

public class SpiritProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float damage = 1f;
    public LayerMask playerLayer = 1;
    public GameObject hitEffect;
    public bool destroyOnHit = true;

    [Header("Visual Effects")]
    public TrailRenderer trailRenderer;
    public ParticleSystem particles;
    public SpriteRenderer spriteRenderer;
    public float fadeSpeed = 2f;

    // Private variables
    private Vector2 direction;
    private float speed;
    private float lifetime;
    private float spawnTime;
    private bool hasHit = false;

    public void Initialize(Vector2 moveDirection, float moveSpeed, float projectileLifetime)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        lifetime = projectileLifetime;
        spawnTime = Time.time;

        // Set rotation to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Apply velocity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    void Update()
    {
        // Check lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            DestroyProjectile();
        }

        // Move projectile if no rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        // Fade out near end of lifetime
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

        // Check if hit player
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
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
            
            if (destroyOnHit)
            {
                CreateHitEffect();
                DestroyProjectile();
            }
        }
        // Hit solid objects (walls, terrain, etc.)
        else if (other.gameObject.layer != gameObject.layer) // Don't hit other spirits
        {
            hasHit = true;
            CreateHitEffect();
            DestroyProjectile();
        }
    }

    void DamagePlayer(PlayerController2D player)
    {
        // You can implement player damage here
        // For now, we'll just apply knockback
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
            AudioManager.Instance.PlaySFX("PlayerHit");
        }

        Debug.Log("Player hit by spirit projectile!");
    }

    void CreateHitEffect()
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up effect after 2 seconds
        }

        // Play hit sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ProjectileHit");
        }
    }

    void DestroyProjectile()
    {
        // Stop any ongoing effects
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        if (particles != null)
        {
            particles.Stop();
        }

        // Destroy the game object
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // Draw projectile direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, direction * 2f);
    }
}
