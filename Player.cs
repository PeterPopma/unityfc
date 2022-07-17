using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    int number;
    private const int LAYER_SHOOT = 1;
    private const int LAYER_CHEER = 2;
    private Player fellowPlayer;
    private Ball scriptBall;
    private Rigidbody rigidbodyBall;
    private PlayerInput playerInput;
    private Transform transformBall;
    private Transform playerBallPosition;
    private Transform playerCameraRoot;
    private Vector3 initialPosition;
    private Animator animator;
    private AudioSource soundDribble;
    private AudioSource soundShoot;
    private AudioSource soundSteal;
    private float distanceSinceLastDribble;
    private bool hasBall;
    private float timeShot;
    private float stealDelay;       // after the player has lost the ball, he cannot steal it back for some time
    private float cheering;
    private float updateTime;
    private Team team;
    private bool takeFreeKick;
    private bool takeThrowIn;
    private float shootingPower;

    public bool HasBall { get => hasBall; set => hasBall = value; }
    public Transform PlayerBallPosition { get => playerBallPosition; set => playerBallPosition = value; }
    public Transform PlayerCameraRoot { get => playerCameraRoot; set => playerCameraRoot = value; }
    public Team Team { get => team; set => team = value; }
    public Player FellowPlayer { get => fellowPlayer; set => fellowPlayer = value; }
    public Vector3 InitialPosition { get => initialPosition; set => initialPosition = value; }
    public bool TakeFreeKick { get => takeFreeKick; set => takeFreeKick = value; }
    public bool TakeThrowIn { get => takeThrowIn; set => takeThrowIn = value; }
    public int Number { get => number; set => number = value; }
    public PlayerInput PlayerInput { get => playerInput; set => playerInput = value; }
    public float ShootingPower { get => shootingPower; set => shootingPower = value; }

    void Awake()
    {
        transformBall = GameObject.Find("Ball").transform;
        scriptBall = transformBall.GetComponent<Ball>();
        soundDribble = GameObject.Find("Sound/dribble").GetComponent<AudioSource>();
        soundShoot = GameObject.Find("Sound/shoot").GetComponent<AudioSource>();
        soundSteal = GameObject.Find("Sound/woosh").GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        PlayerBallPosition = transform.Find("BallPosition");
        rigidbodyBall = transformBall.gameObject.GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        PlayerCameraRoot = transform.Find("PlayerCameraRoot");
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (cheering > 0)
        {
            cheering -= Time.deltaTime;
        }
        else
        {
            animator.SetLayerWeight(LAYER_CHEER, Mathf.Lerp(animator.GetLayerWeight(LAYER_CHEER), 0f, Time.deltaTime * 10f));
        }

        if (stealDelay > 0) {
            stealDelay -= Time.deltaTime;
        }

        updateTime += Time.deltaTime;
        if (updateTime > 1.0f)
        {
            updateTime = 0f;
        }

        if (HasBall)
        {
            DribbleWithBall();
        }
        else if (Game.Instance.PassDestinationPlayer != fellowPlayer && timeShot == 0 && stealDelay <= 0)
        {
            CheckTakeBall();
        }
        if (timeShot > 0)
        {
            // shoot ball
            if (HasBall/* && Time.time - timeShot > 0.2*/)
            {
                TakeShot();
            }
            // finished kicking animation
            if (Time.time - timeShot > 0.5)
            {
                timeShot = 0;
            }
        }
        else
        {
            animator.SetLayerWeight(LAYER_SHOOT, Mathf.Lerp(animator.GetLayerWeight(LAYER_SHOOT), 0f, Time.deltaTime * 10f));
        }
    }

    private void CheckTakeBall()
    {
        float distanceToBall = Vector3.Distance(transformBall.position, PlayerBallPosition.position);
        if (distanceToBall < 0.6)
        {
            if (Game.Instance.PlayerWithBall != null)
            {
                soundSteal.Play();
                Game.Instance.PlayerWithBall.LooseBall(true);
            }
            Game.Instance.SetPlayerWithBall(this);
        }
    }

    private void DribbleWithBall()
    {
        transformBall.position = PlayerBallPosition.position;
        distanceSinceLastDribble += scriptBall.Speed.magnitude * Time.deltaTime;
        if (distanceSinceLastDribble > 3)
        {
            soundDribble.Play();
            distanceSinceLastDribble = 0;
        }
    }

    public void LooseBall(bool stolen = false)
    {
        if (stolen)
        {
            stealDelay = 2.0f;
        }
        HasBall = false;
        shootingPower = 0;
        Game.Instance.RemovePowerBar();
    }

    public void ScoreGoal()
    {
        cheering = 2.0f;
        animator.SetLayerWeight(LAYER_CHEER, 1f);
    }

    public void Shoot()
    {
        if (HasBall)
        {
            timeShot = Time.time;
            animator.Play("Shoot", LAYER_SHOOT, 0f);
            animator.SetLayerWeight(LAYER_SHOOT, 1f);
        }
    }

    private void TakeShot()
    {
        soundShoot.Play();
        Game.Instance.SetPlayerWithBall(null);
        Vector3 shootdirection = transform.forward;
        shootdirection.y += 0.2f;
        rigidbodyBall.AddForce(shootdirection * (10 + shootingPower * 20f), ForceMode.Impulse);
        LooseBall();
    }

    public void Pass()
    {
        if (HasBall && Game.Instance.PassDestinationPlayer == null)
        {
            transform.LookAt(fellowPlayer.transform.position);
            timeShot = Time.time;
            soundShoot.Play();
            LooseBall();
            animator.Play("Shoot", LAYER_SHOOT, 0f);
            animator.SetLayerWeight(LAYER_SHOOT, 1f);
            scriptBall.Pass(this);
        }
    }

    public void Activate()
    {
        Game.Instance.ActiveHumanPlayer = this;
        playerInput.enabled = true;
    }

    public void SetPosition(Vector3 position)
    {
        if (Team.IsHuman)
        {
            GetComponent<CharacterController>().enabled = false;
            transform.position = position;
            GetComponent<CharacterController>().enabled = true;
        }
        else
        {
            transform.position = position;
        }
    }
}
