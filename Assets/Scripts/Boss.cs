using UnityEngine;

public class Boss : RobotSamurai
{
    public Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        faceDirection = -1;
    }

    // Update is called once per frame

    [SerializeField] private float PreferableDistance = 2f; //The distance at which the Boss will attempt to stay at relative to the player
    [SerializeField] private 


    float timer = 0;
    void Update()
    {
        if (timer <= 0)
        {
            HighAttack();
            timer = 1;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }
}
