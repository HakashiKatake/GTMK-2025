using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool destroyOnDeath = false;

    [Header("Damage Settings")]
    public float invulnerabilityTime = 1f;
    public LayerMask damageLayer = -1;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    public float damageFlashDuration = 0.1f;

    [Header("Events")]
    public UnityEvent OnDamaged;
    public UnityEvent OnHealed;
    public UnityEvent OnDeath;
    public UnityEvent<float> OnHealthChanged;

    // Private variables
    private float lastDamageTime;
    private Color originalColor;
    private bool isInvulnerable = false;

    void Start()
    {
        
        if (currentHealth <= 0)
            currentHealth = maxHealth;

       
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

       
        OnHealthChanged.Invoke(currentHealth / maxHealth);
    }

    public void TakeDamage(float damage)
    {
        
        if (isInvulnerable || Time.time - lastDamageTime < invulnerabilityTime)
            return;

       
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        lastDamageTime = Time.time;
        isInvulnerable = true;

       
        StartCoroutine(DamageFlash());

       
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("PlayerHit");
        }

     
        OnDamaged.Invoke();
        OnHealthChanged.Invoke(currentHealth / maxHealth);

        
        if (currentHealth <= 0)
        {
            Die();
        }

        
        Invoke(nameof(EndInvulnerability), invulnerabilityTime);

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }

    public void Heal(float amount)
    {
        if (currentHealth >= maxHealth) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);

        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Heal");
        }

       
        OnHealed.Invoke();
        OnHealthChanged.Invoke(currentHealth / maxHealth);

        Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
    }

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged.Invoke(currentHealth / maxHealth);
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} has died!");

        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Death");
        }

       
        OnDeath.Invoke();

        
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
        else
        {
            // Disable relevant components
            GetComponent<Collider2D>()?.gameObject.SetActive(false);
            
            
        }
    }

    void EndInvulnerability()
    {
        isInvulnerable = false;
    }

    System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        // Flash to damage color
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);

        // Return to original color
        spriteRenderer.color = originalColor;
    }

    // Public getters
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

 
   
}
