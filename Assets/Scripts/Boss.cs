using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss : RobotSamurai
{
    [Header("References")]
    public Player player;
    public LayerMask playerDetectionMap;
    public Collider2D cutsceneTrap;
    public Boss fakePlayerInstance;
    public LevelTransitionManager fadeManagerForgiveMe;
    public Collider2D colliderColliderCollider;

    [Header("Boss AI")]
    [SerializeField] private float PreferableDistance = 2f;

    [Header("End Cutscene")]
    public bool fakePlayer = false;
    public bool showStarted = false;
    public bool goalreached = false;
    public float walkUntilDistanceIs = 1.5f;

    [SerializeField] private float cutsceneEndDelay = 1f;
    [SerializeField] private float fadeDuration = 5f;

    private bool deathSequenceStarted = false;
    private bool cutsceneEnabled = false;
    private bool cutsceneEnded = false;

    private Action currentAction = new Action("None", 0, 0);
    private float currentActionDuration = 0f;

    private readonly List<Action> actions = new List<Action>()
    {
        new Action("HighAttack"),
        new Action("LowAttack"),
        new Action("Parry"),
        new Action("Jump"),
        new Action("WalkForward", .75f),
        new Action("Bait", .5f)
    };

    protected override void Die()
    {
        if (deathSequenceStarted)
        {
            return;
        }

        deathSequenceStarted = true;

        StopAllCoroutines();

        currentAction = new Action("None", 0, 0);
        currentActionDuration = 0f;

        animator.Play("Base Layer.DeadDrop");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (colliderColliderCollider != null)
        {
            colliderColliderCollider.isTrigger = true;
        }

        StartCoroutine(BossDeathSequence());
    }

    private IEnumerator BossDeathSequence()
    {
        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<Player>();
        }

        if (fadeManagerForgiveMe == null)
        {
            fadeManagerForgiveMe = UnityEngine.Object.FindFirstObjectByType<LevelTransitionManager>();
        }

        if (player != null)
        {
            player.SetControlsEnabled(false);
        }

        if (fakePlayerInstance != null)
        {
            Cutscene();

            float maxWaitTime = 8f;
            float timer = 0f;

            while (fakePlayerInstance != null && !fakePlayerInstance.goalreached && timer < maxWaitTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(cutsceneEndDelay);

        cutsceneEnded = true;

        if (fadeManagerForgiveMe != null)
        {
            yield return StartCoroutine(fadeManagerForgiveMe.Fade(0f, 1f, fadeDuration));
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.LogError("Boss: Nie znaleziono LevelTransitionManager na scenie. Fadeout nie mo¿e siê uruchomiæ.");
        }
    }

    protected void Cutscene()
    {
        Debug.Log("CUTSCENE");

        cutsceneEnabled = true;

        if (fakePlayerInstance != null)
        {
            fakePlayerInstance.gameObject.SetActive(true);
            fakePlayerInstance.showStarted = true;
            fakePlayerInstance.goalreached = false;
        }

        if (player != null)
        {
            player.SetControlsEnabled(false);
        }
    }

    void Start()
    {
        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<Player>();
        }

        if (fadeManagerForgiveMe == null)
        {
            fadeManagerForgiveMe = UnityEngine.Object.FindFirstObjectByType<LevelTransitionManager>();
        }

        if (player != null)
        {
            player.boss = this;

            if (player.transform.position.x < transform.position.x)
            {
                faceDirection = -1;
            }
            else
            {
                faceDirection = 1;
            }
        }
        else
        {
            faceDirection = -1;
        }

        AbilityChoiceManager.ApplySavedAbilitiesToBoss(this);

        RefreshFacingDirection();
    }

    IEnumerator _walkConstant(int h, bool bait)
    {
        Action oldAction = currentAction;

        while (currentActionDuration > 0f && currentAction.name == oldAction.name)
        {
            yield return null;

            if (deathSequenceStarted)
            {
                yield break;
            }

            if (rb != null && rb.bodyType == RigidbodyType2D.Static)
            {
                yield break;
            }

            if (player == null)
            {
                yield break;
            }

            if (bait && Math.Abs(player.transform.position.x - transform.position.x) <= PreferableDistance)
            {
                h *= -1;
            }

            Walk(faceDirection * h);

            if (Math.Abs(player.transform.position.x - transform.position.x) <= 1.5f)
            {
                Walk(faceDirection * -1);
            }
        }
    }

    public void WalkForward()
    {
        if (deathSequenceStarted)
        {
            return;
        }

        StartCoroutine(_walkConstant(1, false));
    }

    public void WalkBackwards()
    {
        if (deathSequenceStarted)
        {
            return;
        }

        StartCoroutine(_walkConstant(-1, false));
    }

    public void Bait()
    {
        if (deathSequenceStarted)
        {
            return;
        }

        StartCoroutine(_walkConstant(-1, true));
    }

    IEnumerator currentActionCountdownorsomthishngs(float duration)
    {
        Action oldAction = currentAction;
        currentActionDuration = duration;

        while (true)
        {
            yield return null;

            if (deathSequenceStarted)
            {
                yield break;
            }

            currentActionDuration -= Time.deltaTime;

            if (oldAction.name != currentAction.name)
            {
                yield break;
            }

            if (currentActionDuration <= 0)
            {
                currentAction = new Action("None", 0, 0);
                break;
            }
        }
    }

    public void ChooseAnAction()
    {
        if (deathSequenceStarted)
        {
            return;
        }

        List<Action> possibleActions = new List<Action>(actions);

        if (HasAbility(SamuraiAbility.Dash))
        {
            possibleActions.Add(new Action("Dash", .2f));
        }

        if (HasAbility(SamuraiAbility.DoubleJump))
        {
            possibleActions.Add(new Action("DoubleJump", .1f));
        }

        int r = UnityEngine.Random.Range(0, possibleActions.Count);
        Action action = possibleActions[r];

        if (transform.position.x > 14 && (action.name == "WalkBackwards" || action.name == "Bait"))
        {
            return;
        }
        currentAction = action;
        StartCoroutine(currentActionCountdownorsomthishngs(action.duration));
        Invoke(action.name, 0);
    }

    private struct Action
    {
        public string name;
        public float duration;
        public float chance;

        public Action(string _name)
        {
            name = _name;
            duration = .1f;
            chance = 1f;
        }

        public Action(string _name, float _duration)
        {
            name = _name;
            duration = _duration;
            chance = 1f;
        }

        public Action(string _name, float _duration, float _chance)
        {
            name = _name;
            duration = _duration;
            chance = _chance;
        }
    }

    new void Update()
    {
        if (fakePlayer)
        {
            UpdateFakePlayer();
            return;
        }

        base.Update();

        if (deathSequenceStarted || Health <= 0)
        {
            return;
        }

        UpdateBossAI();
    }

    private void UpdateFakePlayer()
    {
        if (!showStarted)
        {
            return;
        }

        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<Player>();

            if (player == null)
            {
                return;
            }
        }

        if (rb != null && rb.bodyType == RigidbodyType2D.Static)
        {
            return;
        }

        Walk(faceDirection);

        if (Math.Abs(transform.position.x - player.transform.position.x) <= walkUntilDistanceIs)
        {
            showStarted = false;
            goalreached = true;
            Walk(0);
        }
    }

    private void UpdateBossAI()
    {
        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<Player>();

            if (player != null)
            {
                player.boss = this;
            }
        }

        if (player != null)
        {
            if (player.transform.position.x < transform.position.x)
            {
                faceDirection = -1;
            }
            else
            {
                faceDirection = 1;
            }
        }

        if (currentActionDuration > 0)
        {
            return;
        }

        ChooseAnAction();
    }
}