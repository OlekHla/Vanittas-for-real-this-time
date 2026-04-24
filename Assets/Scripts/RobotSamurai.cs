using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;



public class RobotSamurai : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] private Collider2D groundDetection;
    [SerializeField] private LayerMask groundDetectionLayerMask;
    [SerializeField] private Hitbox hitbox;

    protected Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Boolean spriteFacesRightByDefault = true;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip highAttackSound;
    [SerializeField] private AudioClip lowAttackSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip parrySound;
    [SerializeField] private AudioClip successfulParrySound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;

    [SerializeField] private float audioVolume = 1f;
    [SerializeField] private float footstepSoundDelay = .25f;

    [SerializeField] private List<SamuraiAbility> abilities = new List<SamuraiAbility>();

    [SerializeField] private float DashSpeed = 12f;
    [SerializeField] private float DashDuration = .15f;
    [SerializeField] private float DashCooldown = .75f;

    [SerializeField] private float GroundSpeed;
    [SerializeField] private float AirSpeed;
    [SerializeField] private float JumpForce;

    [SerializeField] protected float Health;
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
    private Boolean canDoubleJump = false;
    private float footstepSoundTimer = 0f;
    private float dashCooldownTimer = 0f;


    public enum State
    {
        None,
        HighAttack,
        LowAttack,
        Blocking,
        Parrying,
        Dashing,

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

        if (animator == null)
        {
            animator = gameObject.GetComponent<Animator>();
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

            canDoubleJump = HasAbility(SamuraiAbility.DoubleJump);
        }
    }

    public void ClearAbilities()
    {
        abilities.Clear();
        canDoubleJump = false;
    }

    public void AddAbility(SamuraiAbility ability)
    {
        if (ability == SamuraiAbility.None)
        {
            return;
        }

        if (abilities.Contains(ability) == true)
        {
            return;
        }

        abilities.Add(ability);

        if (ability == SamuraiAbility.DoubleJump && onGround == true)
        {
            canDoubleJump = true;
        }
    }

    public Boolean HasAbility(SamuraiAbility ability)
    {
        return abilities.Contains(ability);
    }

    public void UseAbility(SamuraiAbility ability)
    {
        if (ability == SamuraiAbility.Dash)
        {
            Dash();
        }
        else if (ability == SamuraiAbility.DoubleJump)
        {
            DoubleJump();
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

    public void SetState(string newState)
    {
        Enum.TryParse(newState, out state);
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

        animator.Play("Base Layer.Parry");
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

    public void EnableHitbox()
    {
        hitbox.Enable();
    }
    public void DisableHitbox()
    {
        hitbox.Disable();
    }

    public void Explode()
    {
        Debug.Log("GJWIOGJIWOJGE");
    }

    protected virtual void HighAttack()
    {
        if (controlsEnabled == false) { return; }
        if (state != State.None) { return; }

        PlaySound(highAttackSound);

        hitbox.transform.localScale = HighHitboxSize;
        hitbox.transform.localPosition = new Vector3((HighHitboxSize.x / 2 + HighHitboxOffset.x + .5f) * faceDirection, HighHitboxSize.y / 2 + HighHitboxOffset.y, 0);

        animator.Play("Base Layer.HighAttack");
    }
    protected virtual void LowAttack()
    {
        if (controlsEnabled == false) { return; }
        if (state != State.None) { return; }

        PlaySound(lowAttackSound);

        hitbox.transform.localScale = LowHitboxSize;
        hitbox.transform.localPosition = new Vector3((LowHitboxSize.x / 2 + LowHitboxOffset.x + .5f) * faceDirection, LowHitboxSize.y / 2 + LowHitboxOffset.y, 0);

        animator.Play("Base Layer.LowAttack");
    }

    public void Dash()
    {
        if (controlsEnabled == false) { return; }
        if (HasAbility(SamuraiAbility.Dash) == false) { return; }
        if (state != State.None) { return; }
        if (dashCooldownTimer > 0f) { return; }

        StartCoroutine(DashRoutine());
    }

    protected IEnumerator DashRoutine()
    {
        state = State.Dashing;
        dashCooldownTimer = DashCooldown;

        PlaySound(dashSound);

        float timer = 0f;

        while (timer < DashDuration)
        {
            yield return null;

            timer += Time.deltaTime;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(DashSpeed * faceDirection, rb.linearVelocity.y);
            }
        }

        if (state == State.Dashing)
        {
            state = State.None;
        }
    }

    public void DoubleJump()
    {
        if (controlsEnabled == false) { return; }
        if (HasAbility(SamuraiAbility.DoubleJump) == false) { return; }
        if (onGround == true) { return; }

        Jump();
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

        if (targetSamurai == null)
        {
            return;
        }

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
            Debug.Log("bruh.");
            if (targetSamurai.onGround == false)
            {
                Debug.Log("sfjwioqjgwqg");
                //Jumped over ggez
            }
            else
            {
                PlaySound(hitSound);

                targetSamurai.TakeDamage(1);
                StartCoroutine(targetSamurai.SetStateForDuration(State.Stunned, .1f));
            }
        }
    }

    public void Walk(float h)
    {
        if (controlsEnabled == false) { return; }

        if (state != State.None)
        {
            if (state != State.Dashing && rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }

            return;
        }

        animator.SetBool("Walking", Math.Round(h) != 0);

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

            if (HasAbility(SamuraiAbility.DoubleJump) == true)
            {
                canDoubleJump = true;
            }
        }
        else if (HasAbility(SamuraiAbility.DoubleJump) == true && canDoubleJump == true)
        {
            canDoubleJump = false;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }

            PlaySound(doubleJumpSound);
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        animator.SetFloat("YVelocity", rb.linearVelocity.y);

        /*switch (state)
        {
            case State.None:
                {
                    animator.SetInteger("CurrentState", 0);
                    break;
                }
            case State.HighAttack:
                {
                    animator.SetInteger("CurrentState", 1);
                    break;
                }
            default:
                break;
        }*/
    }

    private void LateUpdate()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

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
            canDoubleJump = HasAbility(SamuraiAbility.DoubleJump);
            PlaySound(landSound);
        }

        RefreshFacingDirection();
    }
}