using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss : RobotSamurai
{
    public Player player;

    public LayerMask playerDetectionMap;
    public Collider2D cutsceneTrap;
    public Boss fakePlayerInstance;
    public LevelTransitionManager fadeManagerForgiveMe;

    public Collider2D colliderColliderCollider;
    protected override void Die()
    {
        //Cutscene
        animator.Play("Base Layer.DeadDrop");
        rb.bodyType = RigidbodyType2D.Static;
        colliderColliderCollider.isTrigger = true;
        rb.linearVelocity = new Vector2(0, 0);
    }

    private bool cutsceneEnabled = false;
    private bool cutsceneEnded = false;
    protected void Cutscene()
    {
        Debug.Log("CUTSCENE");
        cutsceneEnabled = true;
        fakePlayerInstance.gameObject.SetActive(true);
        fakePlayerInstance.showStarted = true;
        player.SetControlsEnabled(false);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<Player>();
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

    // Update is called once per frame

    [SerializeField] private float PreferableDistance = 2f; //The distance at which the Boss will attempt to stay at relative to the player

    private Action currentAction = new Action("None", 0, 0); //Defines the current action chosen by NPC to do (tells the code to not start another one until this one is over)
    private float currentActionDuration = 0f; //Defines how long until the current action is over and it expires

    IEnumerator _walkConstant(int h, bool bait)
    {
        Action oldAction = currentAction;
        while (currentActionDuration > 0f && currentAction.name == oldAction.name)
        {
            yield return null;

            if (player == null)
            {
                yield break;
            }

            if (bait & Math.Abs(player.transform.position.x - transform.position.x) <= PreferableDistance)
            {
                h *= -1;
            }

            Walk(faceDirection * h);

            if (Math.Abs(player.transform.position.x - transform.position.x) <= 1.5f) //if too close then GET AWAY
            {
                Walk(faceDirection * -1);
            }
        }
    }

    public void WalkForward()
    {
        StartCoroutine(_walkConstant(1, false));
    }

    public void WalkBackwards()
    {
        StartCoroutine(_walkConstant(-1, false));
    }

    public void Bait()
    {

        StartCoroutine(_walkConstant(-1, true));
        //Approach the player and move away as soon as they reach preferable distance
    }

    IEnumerator currentActionCountdownorsomthishngs(float duration)
    {
        Action oldAction = currentAction;
        currentActionDuration = duration;
        while (true)
        {
            yield return null; //wait a frame
            currentActionDuration -= Time.deltaTime;
            if (oldAction.name != currentAction.name)
            {
                Debug.Log("Bruh");
                Debug.Log(currentAction.name);
                yield break; //If action updated, disregard this counter
            }
            if (currentActionDuration <= 0)
            {
                Debug.Log("Sigma");
                currentAction = new Action("None", 0, 0);
                break; //Exit the loop
            }
        }
    }

    public void ChooseAnAction() //Get a random action and activate it
    {
        List<Action> possibleActions = new List<Action>(actions);

        if (HasAbility(SamuraiAbility.Dash) == true)
        {
            possibleActions.Add(new Action("Dash", .2f));
        }

        if (HasAbility(SamuraiAbility.DoubleJump) == true)
        {
            possibleActions.Add(new Action("DoubleJump", .1f));
        }

        int r = UnityEngine.Random.Range(0, possibleActions.Count);
        Action action = possibleActions[r];

        Debug.Log(action.name);
        currentAction = action;
        StartCoroutine(currentActionCountdownorsomthishngs(action.duration));
        Invoke(action.name, 0);
    }

    private struct Action //Slapped together to hold data like action name, action duration and action chance ig
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

    List<Action> actions = new List<Action>() { new Action("HighAttack"), new Action("LowAttack"), new Action("Parry"), new Action("Jump"), new Action("WalkForward", .75f), new Action("WalkBackwards", .75f), new Action("Bait", 1f) };


    public bool fakePlayer = false;
    public bool showStarted = false;
    public bool goalreached = false;
    public float walkUntilDistanceIs = 10;

    private float cutsceneEndTimer = 1f;

    new void Update()
    {
        if (fakePlayer)
        {
            if (showStarted)
            {
                Walk(faceDirection); //Walk forward
                if(Math.Abs(transform.position.x - player.transform.position.x) <= walkUntilDistanceIs) //Until a certain preset point
                {
                    showStarted = false; //and then stop
                    goalreached = true;
                    Walk(0);
                    return;
                }
            }
            return;
        }

        base.Update();

        List<Collider2D> res = new List<Collider2D>();
        ContactFilter2D groundContactFilter = new ContactFilter2D();
        groundContactFilter.layerMask = playerDetectionMap;
        groundContactFilter.useLayerMask = true;
        int hits = cutsceneTrap.Overlap(groundContactFilter, res);

        if(!cutsceneEnabled && hits > 0 && Health <= 0)
        {
            Cutscene();
        }
        if (cutsceneEnabled && !cutsceneEnded && fakePlayerInstance.goalreached)
        {
            cutsceneEndTimer -= Time.deltaTime;
            if(cutsceneEndTimer <= 0)
            {
                cutsceneEnded = true;
                StartCoroutine(fadeManagerForgiveMe.Fade(0f, 1f, 5));
            }
        }

        if(Health <= 0)
        {
            return;
        }

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