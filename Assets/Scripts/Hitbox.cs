using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public GameObject owner;
    public string attackType = "High";

    new private Collider2D collider;
    protected List<GameObject> hitTargets = new List<GameObject>();

    void Awake()
    {
        collider = GetComponent<Collider2D>();
    }

    protected virtual void ResolveCollision(Collider2D col)
    {
        if (owner == null)
        {
            return;
        }

        RobotSamurai ownerSamurai = owner.GetComponent<RobotSamurai>();

        if (ownerSamurai == null)
        {
            return;
        }

        Debug.Log("HIT");
        ownerSamurai.OnLandHit(col);
    }

    public virtual void Disable()
    {
        gameObject.SetActive(false);
        hitTargets.Clear();
    }

    public virtual void Enable()
    {
        gameObject.SetActive(true);
    }

    public IEnumerator EnableForDuration(float duration)
    {
        Enable();
        yield return new WaitForSeconds(duration);
        Disable();
    }

    protected void Update()
    {

        if (!gameObject.activeSelf)
        {
            return;
        }

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

            if (IsOwnerCollider(col) == true)
            {
                continue;
            }

            Hitbox otherHitbox = col.GetComponentInParent<Hitbox>();

            if (otherHitbox != null)
            {
                continue;
            }

            ScrapPile scrapPile = col.GetComponentInParent<ScrapPile>();

            if (scrapPile != null)
            {
                GameObject scrapPileObject = scrapPile.gameObject;

                if (hitTargets.Contains(scrapPileObject) == true)
                {
                    continue;
                }

                hitTargets.Add(scrapPileObject);
                scrapPile.Hit(owner);
                continue;
            }

            GameObject targetOwner = col.gameObject;

            if (targetOwner.tag != "Entity" && targetOwner.tag != "Player")
            {
                continue;
            }

            if (hitTargets.Contains(targetOwner) == true)
            {
                continue;
            }

            hitTargets.Add(targetOwner);
            ResolveCollision(col);
        }
    }

    private bool IsOwnerCollider(Collider2D col)
    {
        if (owner == null)
        {
            return false;
        }

        if (col.gameObject == owner)
        {
            return true;
        }

        if (col.transform.IsChildOf(owner.transform) == true)
        {
            return true;
        }

        return false;
    }
}