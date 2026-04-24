using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : RobotSamurai
{
    public Boss boss; //Set this when entering boss room. Used so player always faces the boss

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (boss == null)
        {
            boss = UnityEngine.Object.FindFirstObjectByType<Boss>();
        }
    }

    protected override void Die()
    {
        //Restart level
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (controlsEnabled == false)
        {
            return;
        }

        if (boss == null)
        {
            boss = UnityEngine.Object.FindFirstObjectByType<Boss>();
        }

        var h = Input.GetAxis("Horizontal");

        if (boss != null)
        {
            if (boss.transform.position.x < transform.position.x)
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
            if (h < 0)
            {
                faceDirection = -1;
            }
            else if (h > 0)
            {
                faceDirection = 1;
            }
        }

        RefreshFacingDirection();

        if (Input.GetMouseButtonDown(0)) //left click
        {
            Vector2 cursorPos = Input.mousePosition;
            float cursorHeightPercent = Input.mousePosition.y / Screen.height;
            if (cursorHeightPercent <= .4)
            {
                LowAttack();
            }
            else
            {
                HighAttack();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Parry();
        }
        base.Walk(h);

        if (Input.GetButtonDown("Jump"))
        {
            base.Jump();
        }
    }
}