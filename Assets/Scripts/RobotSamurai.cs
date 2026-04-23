using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;



public class RobotSamurai : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D groundDetection;
    [SerializeField] private LayerMask groundDetectionLayerMask;
    [SerializeField] private Hitbox hitbox;

    [SerializeField] private float GroundSpeed;
    [SerializeField] private float AirSpeed;
    [SerializeField] private float JumpForce;

    [SerializeField] private float Health;
    [SerializeField] private float MaxHealth;

    [SerializeField] private float ParryWindow = 1f;

    [SerializeField] private Vector2 HighHitboxSize;
    [SerializeField] private Vector2 HighHitboxOffset;

    [SerializeField] private Vector2 LowHitboxSize;
    [SerializeField] private Vector2 LowHitboxOffset;

    protected Boolean onGround = true;
    protected State state = State.None;
    protected int faceDirection = 1; //1 = right. -1 = left.


    public enum State
    {
        None,
        HighAttack,
        LowAttack,
        Blocking,
        Parrying,

        Stunned
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    protected IEnumerator SetStateForDuration(State newState, float duration)
    {
        state = newState;
        float timer = 0f;
        while (true)
        {
            yield return null; //wait a frame
            timer += Time.deltaTime;
            if(state != newState)
            {
                yield break; //If state updated, disregard this counter
            }
            if(timer >= duration)
            {
                state = State.None;
                break; //Exit the loop and reset the state yay
            }
        }
    }

    public void SetState(State newState) //Call this in animation events
    {
        state = newState;
    }

    protected virtual void Parry()
    {
        if (state != State.None || onGround == false) { return; }
        Debug.Log("Parrying");
        StartCoroutine(SetStateForDuration(State.Parrying, ParryWindow));
    }

    protected virtual void OnParry()
    {
        Debug.Log("Hah! gitgud!");
        state = State.None; //Reset state, so the one who parried can instantly counter-attack

        //If we (somehow) find the time and our artists don't die from death, we could get an animation for getting parried (could be literally one frame)
        //Where it's just the samurai having it's katana pushed aside, leaving it exposed for a counter attack
        //And during that time it would be afflicted with a state effect which would leave it unable to do anything for the length of the animation, letting player get a free hit in
    }

    protected virtual void HighAttack()
    {
        if (state != State.None) { return;  }
        hitbox.transform.localScale = HighHitboxSize;
        hitbox.transform.localPosition = new Vector3((HighHitboxSize.x / 2 + HighHitboxOffset.x + .5f) * faceDirection, HighHitboxSize.y / 2 + HighHitboxOffset.y, 0);
        StartCoroutine(SetStateForDuration(State.HighAttack, .25f));
        StartCoroutine(hitbox.EnableForDuration(.1f));
    }
    protected virtual void LowAttack()
    {
        if (state != State.None) { return; }
        hitbox.transform.localScale = LowHitboxSize;
        hitbox.transform.localPosition = new Vector3((LowHitboxSize.x / 2 + LowHitboxOffset.x + .5f) * faceDirection, LowHitboxSize.y / 2 + LowHitboxOffset.y, 0);
        StartCoroutine(SetStateForDuration(State.LowAttack, .25f));
        StartCoroutine(hitbox.EnableForDuration(.1f));
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float amount)
    {
        //play animation
        Health = Mathf.Clamp(Health - amount, 0, MaxHealth);
        Debug.Log(Health);
        if (Health <= 0)
        {
            Die();
        }
    }

    public void OnLandHit(Collider2D col)
    {
        RobotSamurai targetSamurai = col.gameObject.GetComponent<RobotSamurai>();
        if (state == State.HighAttack)
        {
            if (targetSamurai.state == State.Parrying)
            {
                //Parried ggez
                targetSamurai.OnParry();
            }
            else
            {
                targetSamurai.TakeDamage(1);
                StartCoroutine(targetSamurai.SetStateForDuration(State.Stunned, .1f));
            }
        }
        else if (state == State.LowAttack)
        {
            if(targetSamurai.onGround == false)
            {
                //Jumped over ggez
            }
            else
            {
                targetSamurai.TakeDamage(1);
                StartCoroutine(targetSamurai.SetStateForDuration(State.Stunned, 5f));
            }
        }
    }

    public void Walk(float h)
    {
        if(state != State.None) { rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); return; }

        //Play animation
        if (onGround)
        {
            rb.linearVelocity = new Vector2(GroundSpeed * h, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(AirSpeed * h, rb.linearVelocity.y);
        }
    }

    public void Jump()
    {
        if (state != State.None) { return; }

        //Play animation
        //Change state
        if (onGround)
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LateUpdate()
    {
        //onGround = true;
        List<Collider2D> res = new List<Collider2D>();
        ContactFilter2D groundContactFilter = new ContactFilter2D();
        groundContactFilter.layerMask = groundDetectionLayerMask;
        groundContactFilter.useLayerMask = true;
        int hits = groundDetection.Overlap(groundContactFilter, res);

        onGround = hits > 0;
    }
}
