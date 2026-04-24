using System.Collections.Generic;
using UnityEngine;

public class ScrapPile : MonoBehaviour
{
    [Header("¯ycie sterty œmieci")]
    [SerializeField] private int maxHits = 3;
    [SerializeField] private int currentHits = 0;

    [Header("Nagroda")]
    [SerializeField] private int scrapReward = 5;

    [Header("Obra¿enia dla gracza po wejœciu")]
    [SerializeField] private float damageToPlayer = 1f;
    [SerializeField] private float damageCooldown = 0.75f;

    [Header("Odepchniêcie gracza")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float upwardKnockbackForce = 1f;

    [Header("Opcje")]
    [SerializeField] private bool destroyAfterBreaking = true;

    private float nextDamageTime = 0f;
    private bool isBroken = false;

    private HashSet<Collider2D> playerTouchingColliders = new HashSet<Collider2D>();

    public void Hit(GameObject attacker)
    {
        if (isBroken == true)
        {
            return;
        }

        currentHits++;

        Debug.Log("Trafiono stertê œmieci: " + currentHits + " / " + maxHits);

        if (currentHits >= maxHits)
        {
            BreakPile();
        }
    }

    private void BreakPile()
    {
        if (isBroken == true)
        {
            return;
        }

        isBroken = true;

        if (ScrapManager.Instance != null)
        {
            ScrapManager.Instance.AddScrap(scrapReward);
        }
        else
        {
            Debug.LogWarning("Nie ma ScrapManager na scenie. Nie dodano z³omu.");
        }

        if (destroyAfterBreaking == true)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RegisterPlayerTouch(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        UnregisterPlayerTouch(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        RegisterPlayerTouch(collision.collider);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        UnregisterPlayerTouch(collision.collider);
    }

    private void RegisterPlayerTouch(Collider2D other)
    {
        if (isBroken == true)
        {
            return;
        }

        if (ShouldIgnoreCollider(other) == true)
        {
            return;
        }

        Player player = other.GetComponentInParent<Player>();

        if (player == null)
        {
            return;
        }

        bool playerWasAlreadyTouching = playerTouchingColliders.Count > 0;

        playerTouchingColliders.Add(other);

        if (playerWasAlreadyTouching == true)
        {
            return;
        }

        TryDamagePlayer(player);
    }

    private void UnregisterPlayerTouch(Collider2D other)
    {
        if (ShouldIgnoreCollider(other) == true)
        {
            return;
        }

        Player player = other.GetComponentInParent<Player>();

        if (player == null)
        {
            return;
        }

        if (playerTouchingColliders.Contains(other) == true)
        {
            playerTouchingColliders.Remove(other);
        }
    }

    private bool ShouldIgnoreCollider(Collider2D other)
    {
        if (other == null)
        {
            return true;
        }

        Hitbox hitbox = other.GetComponentInParent<Hitbox>();

        if (hitbox != null)
        {
            return true;
        }

        return false;
    }

    private void TryDamagePlayer(Player player)
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        nextDamageTime = Time.time + damageCooldown;

        player.TakeDamage(damageToPlayer);
        KnockbackPlayer(player);
    }

    private void KnockbackPlayer(Player player)
    {
        Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();

        Vector2 knockbackDirection;

        if (player.transform.position.x < transform.position.x)
        {
            knockbackDirection = new Vector2(-1f, upwardKnockbackForce).normalized;
        }
        else
        {
            knockbackDirection = new Vector2(1f, upwardKnockbackForce).normalized;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
        else
        {
            player.transform.position += new Vector3(knockbackDirection.x, knockbackDirection.y, 0f) * 0.5f;
        }
    }
}