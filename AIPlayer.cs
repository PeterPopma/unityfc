using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    [SerializeField] private int playMode;
    private Transform transformBall;
    private Game scriptGame;
    private Player scriptPlayer;
    private Ball scriptBall;
    private Animator animator;
    private Transform playerBallPosition;
    private Vector3 targetGoalLocation;

    // Start is called before the first frame update
    void Start()
    {
        transformBall = GameObject.Find("Ball").transform;
        scriptBall = transformBall.GetComponent<Ball>();
        scriptPlayer = GetComponent<Player>();
        scriptGame = GameObject.Find("Scripts").GetComponent<Game>();
        animator = GetComponent<Animator>();
        playerBallPosition = transform.Find("BallPosition");
        targetGoalLocation = new Vector3(52.3696518f, 0.5f, -0.219999999f);
    }

    // Update is called once per frame
    void Update()
    {
        if (scriptGame.TeamWithBall != scriptPlayer.Team.Number)
        {
            DefendMode();
        }
        else
        {
            AttackMode();
        }
    }

    // players move towards enemy goal, one to z=12, one to z=-12
    private void AttackMode()
    {
        // move to target goal
        transform.LookAt(targetGoalLocation);
        Vector3 movedirection = targetGoalLocation - playerBallPosition.position;
        float distanceToGoal = movedirection.magnitude;
        Vector3 moveSpeed = new Vector3(movedirection.normalized.x * 4f * Time.deltaTime, 0, movedirection.normalized.z * 4f * Time.deltaTime);
        transform.position += moveSpeed;
        animator.SetFloat("Speed", 5);
        animator.SetFloat("MotionSpeed", 1);
        // shoot
        if (distanceToGoal < 15)
        {
            animator.SetFloat("Speed", 0);
            animator.SetFloat("MotionSpeed", 0);
            scriptPlayer.Shoot();
        }
    }

    // player closest to ball tries to steal it
    // player farthest from ball moves between goal and player closest to goal
    private void DefendMode()
    {
        if (scriptGame.PlayerClosestToBall(1) == scriptPlayer)
        {
            MoveToBall();
        }
        else
        {
            MoveToBall();
        }
    }

    private void MoveToBall()
    {
        transform.LookAt(transformBall.position);
        Vector3 movedirection = transformBall.position - playerBallPosition.position;
        Vector3 moveSpeed = new Vector3(movedirection.normalized.x * 5f * Time.deltaTime, 0, movedirection.normalized.z * 5f * Time.deltaTime);
        transform.position += moveSpeed;
        animator.SetFloat("Speed", 8);
        animator.SetFloat("MotionSpeed", 1);
    }
}
