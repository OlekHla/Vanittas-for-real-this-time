using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Hitbox : MonoBehaviour
{
    public GameObject owner;
    public string attackType = "High";

    new private Collider2D collider; //The "new" keyword is there to suppress the icky warning over this variable name hiding an already deprecated property
    protected List<GameObject> hitTargets = new List<GameObject>();

    void Awake()
    {
        collider = GetComponent<Collider2D>();
    }

    protected virtual void ResolveCollision(Collider2D col)
    {
        RobotSamurai ownerSamurai = owner.GetComponent<RobotSamurai>();
        ownerSamurai.OnLandHit(col);
    }

    public virtual void Disable()
    {
        gameObject.SetActive(false);
        hitTargets.Clear(); //Clear list of hit targets (not relevant anymore, since the lifetime of the hitbox has ended)
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
        if (!gameObject.activeSelf) { return; } //If not active, then don't bother
        List<Collider2D> res = new List<Collider2D>();
        collider.Overlap(res);
        foreach (Collider2D col in res)
        { //Iterating over overlapping colliders
            GameObject targetOwner = col.gameObject; //Getting the owner of the collider (in case we have an enemy with several colliders or smth)
            if (targetOwner.tag != "Entity") { continue; }
            if (hitTargets.Contains(targetOwner))
            { //If this target has already been hit during the lifetime of this attack
                continue; //Then ignore it and look at other hit targets
            }

            hitTargets.Add(targetOwner); //Add it to the list of hit targets
            ResolveCollision(col);
        }
    }
}
