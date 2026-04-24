using System.Collections.Generic;
using UnityEngine;

public class DamageOnHit : MonoBehaviour
{
    new private Collider2D collider;

    [Header("Obra¿enia")]
    public float damage = 1f;

    [Header("Cooldown")]
    [SerializeField] private float damageCooldown = 0.25f;

    [Header("Opcjonalny w³aœciciel, którego nie ranimy")]
    [SerializeField] private GameObject owner;

    private float timer = 0f;

    void Awake()
    {
        collider = GetComponent<Collider2D>();
    }

    protected void Update()
    {
        if (collider == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer < damageCooldown)
        {
            return;
        }

        timer = 0f;

        List<Collider2D> results = new List<Collider2D>();
        collider.Overlap(results);

        foreach (Collider2D col in results)
        {
            if (col == null)
            {
                continue;
            }

            if (col == collider)
            {
                continue;
            }

            Hitbox hitbox = col.GetComponentInParent<Hitbox>();

            if (hitbox != null)
            {
                continue;
            }

            RobotSamurai victim = col.GetComponentInParent<RobotSamurai>();

            if (victim == null)
            {
                continue;
            }

            if (owner != null && victim.gameObject == owner)
            {
                continue;
            }

            victim.TakeDamage(damage);
        }
    }
}