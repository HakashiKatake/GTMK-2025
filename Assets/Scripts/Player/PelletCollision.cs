using UnityEngine;
using System.Collections;

public class PelletCollision : MonoBehaviour
{
    void Start()
    {
        // Ensure the existing collider is set as trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. If pellet hits spirit -> spirit flashes red, pellet disappears
        if (other.CompareTag("Spirit"))
        {
            // Make spirit flash red - start coroutine on the spirit instead of pellet
            MonoBehaviour spiritMB = other.GetComponent<MonoBehaviour>();
            if (spiritMB != null)
            {
                spiritMB.StartCoroutine(FlashSpirit(other));
            }
            
            // Damage the spirit
            Health spiritHealth = other.GetComponent<Health>();
            if (spiritHealth != null)
            {
                spiritHealth.TakeDamage(5f);
            }
            
            // Destroy pellet
            Destroy(gameObject);
        }
        // 2. If pellet hits spirit projectile -> both destroy
        else if (other.gameObject.name.Contains("Projectile") || 
                 other.GetComponent<SpiritProjectileLogic>() != null)
                 
        {
            // Destroy the spirit projectile
            Destroy(other.gameObject);
            // Destroy this pellet
            Destroy(gameObject);
        }
        // 3. If pellet hits walls/obstacles -> pellet disappears (but not player or bot)
        else if (!other.CompareTag("Player") && !other.CompareTag("Bot") && other.gameObject.layer != gameObject.layer)
        {
            // Only destroy if it's a solid object (has a non-trigger collider or specific tags)
            if (!other.isTrigger)
            {
                Destroy(gameObject);
            }
        }
    }

    static System.Collections.IEnumerator FlashSpirit(Collider2D spirit)
    {
        SpriteRenderer renderer = spirit.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.color;
            renderer.color = Color.red;
            
            yield return new WaitForSeconds(0.5f);
            
            renderer.color = originalColor;
        }
    }
}
