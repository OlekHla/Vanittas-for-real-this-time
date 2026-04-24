using System.Collections.Generic;
using UnityEngine;

public class DamageOnHit : MonoBehaviour
{
    new private Collider2D collider; //The "new" keyword is there to suppress the icky warning over this variable name hiding an already deprecated property
    public float damage = 1;
    [SerializeField] private float damageCooldown = 0.25f;
    private float timer = 0f;

    void Awake()
    {
        collider = GetComponent<Collider2D>();
    }


    protected void Update()
    {
        timer += Time.deltaTime;
        if (timer > damageCooldown)
        {
            timer = 0;
            List<Collider2D> res = new List<Collider2D>();
            collider.Overlap(res);
            foreach (Collider2D col in res)
            { //Iterating over overlapping colliders
                GameObject targetOwner = col.gameObject; //Getting the owner of the collider (in case we have an enemy with several colliders or smth)
                if (targetOwner.tag != "Entity" && targetOwner.tag != "Player") { continue; }

                RobotSamurai victim = targetOwner.GetComponent<RobotSamurai>();
                victim.TakeDamage(damage);
            }
        }
    }
}
