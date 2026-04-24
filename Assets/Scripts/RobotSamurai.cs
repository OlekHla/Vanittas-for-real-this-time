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

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Boolean spriteFacesRightByDefault = true;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip highAttackSound;
    [SerializeField] private AudioClip lowAttackSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip parrySound;
    [SerializeField] private AudioClip successfulParrySound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;

    [SerializeField] private float audioVolume = 1f;
    [SerializeField] private float footstepSoundDelay = .25f;

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
    protected Boolean controlsEnabled = true;
    protected State state = State.None;
    protected int faceDirection = 1; //1 = right. -1 = left.

    private Boolean wasOnGround = true;
    private float footstepSoundTimer = 0f;


    public enum State
    {
        None,
        HighAttack,
        LowAttack,
        Blocking,
        Parrying,

        Stunned
    }

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        RefreshFacingDirection();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void SetControlsEnabled(Boolean enabled)
    {
        controlsEnabled = enabled;

        if (enabled == false)
        {
            state = State.None;

            if (hitbox != null)
            {
                hitbox.Disable();
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
        }
        else
        {
            if (rb != null)
            {
                rb.simulated = true;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    protected void RefreshFacingDirection()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (spriteFacesRightByDefault == true)
        {
            spriteRenderer.flipX = faceDirection == -1;
        }
        else
        {
            spriteRenderer.flipX = faceDirection == 1;
        }
    }

    protected void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, audioVolume);
    }

    protected void PlaySoundAtPosition(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clip, transform.position, audioVolume);
    }

    protected void TryPlayFootstepSound(float movementInput)
    {
        if (footstepSound == null)
        {
            return;
        }

        if (onGround == false)
        {
            return;
        }

        if (Mathf.Abs(movementInput) <= .1f)
        {
            footstepSoundTimer = 0f;
            return;
        }

        footstepSoundTimer -= Time.deltaTime;

        if (footstepSoundTimer <= 0f)
        {
            PlaySound(footstepSound);
            footstepSoundTimer = footstepSoundDelay;
        }
    }

    protected IEnumerator SetStateForDuration(State newState, float duration)
    {
        state = newState;
        float timer = 0f;
        while (true)
        {
            yield return null; //wait a frame
            timer += Time.deltaTime;
            if (state != newState)
            {
                yield break; //If state updated, disregard this counter
            }
            if (timer >= duration)
            {
                state = State.None;
                break; //Exit the loop and reset the state yay
            }
        }
    }

    public void SetState(State newState) //Call this in animation events!! (To determine when the attack ends and stuff)
    {
        state = newState;
    }

    protected virtual void Parry()
    {
        if (controlsEnabled == false) { return; }
        if (state != State.None || onGround == false) { return; }

        PlaySound(parrySound);

        Debug.Log("Parrying");
        StartCoroutine(SetStateForDuration(State.Parrying, ParryWindow));
    }

    protected virtual void OnParry()
    {
        PlaySound(successfulParrySound);

        Debug.Log("Hah! gitgud!");
        state = State.None; //Reset state, so the one who parried can instantly counter-attack

        //If we (somehow) find the time and our artists don't die from death, we could get an animation for getting parried (could be literally one frame)
        //Where it's just the samurai having it's katana pushed aside, leaving it exposed for a counter attack
        //And during that time it would be afflicted with a state effect which would leave it unable to do anything for the length of the animation, letting player get a free hit in
    }

    protected virtual void HighAttack()
    {
        if (controlsEnabled == false) { return; }
        if (state != State.None) { return; }

        PlaySound(highAttackSound);

        hitbox.transform.localScale = HighHitboxSize;
        hitbox.transform.localPosition = new Vector3((HighHitboxSize.x / 2 + HighHitboxOffset.x + .5f) * faceDirection, HighHitboxSize.y / 2 + HighHitboxOffset.y, 0);
        StartCoroutine(SetStateForDuration(State.HighAttack, .25f));
        StartCoroutine(hitbox.EnableForDuration(.1f));
    }
    protected virtual void LowAttack()
    {
        if (controlsEnabled == false) { return; }
        if (state != State.None) { return; }

        PlaySound(lowAttackSound);

        hitbox.transform.localScale = LowHitboxSize;
        hitbox.transform.localPosition = new Vector3((LowHitboxSize.x / 2 + LowHitboxOffset.x + .5f) * faceDirection, LowHitboxSize.y / 2 + LowHitboxOffset.y, 0);
        StartCoroutine(SetStateForDuration(State.LowAttack, .25f));
        StartCoroutine(hitbox.EnableForDuration(.1f));
    }

    protected virtual void Die()
    {
        PlaySoundAtPosition(deathSound);
        Destroy(gameObject);
    }

    public void TakeDamage(float amount)
    {
        //play animation
        PlaySound(damageSound);

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
                PlaySound(hitSound);

                targetSamurai.TakeDamage(1);
                StartCoroutine(targetSamurai.SetStateForDuration(State.Stunned, .1f));
            }
        }
        else if (state == State.LowAttack)
        {
            if (targetSamurai.onGround == false)
            {
                //Jumped over ggez
            }
            else
            {
                PlaySound(hitSound);

                targetSamurai.TakeDamage(1);
                StartCoroutine(targetSamurai.SetStateForDuration(State.Stunned, 5f));
            }
        }
    }

    public void Walk(float h)
    {
        if (controlsEnabled == false) { return; }
        if (state != State.None) { rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); return; }

        TryPlayFootstepSound(h);

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
        if (controlsEnabled == false) { return; }
        if (state != State.None) { return; }

        //Play animation
        //Change state
        if (onGround)
        {
            PlaySound(jumpSound);
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LateUpdate()
    {
        wasOnGround = onGround;

        //onGround = true;
        List<Collider2D> res = new List<Collider2D>();
        ContactFilter2D groundContactFilter = new ContactFilter2D();
        groundContactFilter.layerMask = groundDetectionLayerMask;
        groundContactFilter.useLayerMask = true;
        int hits = groundDetection.Overlap(groundContactFilter, res);

        onGround = hits > 0;

        if (onGround == true && wasOnGround == false)
        {
            PlaySound(landSound);
        }

        RefreshFacingDirection();
    }
}