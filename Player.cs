using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private const int LAYER_SHOOT = 1;
    private Game scriptGame;
    private Player fellowPlayer;
    private Ball scriptBall;
    private Rigidbody rigidbodyBall;
    private PlayerInput playerInput;
    private Transform transformBall;
    private Transform playerBallPosition;
    private Transform playerCameraRoot;
    private Vector3 initialPosition;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    private AudioSource soundDribble;
    private AudioSource soundShoot;
    private AudioSource soundSteal;
    private float distanceSinceLastDribble;
    private bool stickBallToPlayer;
    private float timeShot;
    private float stealDelay;       // after the player has lost the ball, he cannot steal it back for some time
    private float updateTime;
    private Team team;
    private bool takeFreeKick;
    private bool takeThrowIn;

    public bool StickBallToPlayer { get => stickBallToPlayer; set => stickBallToPlayer = value; }
    public Transform PlayerBallPosition { get => playerBallPosition; set => playerBallPosition = value; }
    public Transform PlayerCameraRoot { get => playerCameraRoot; set => playerCameraRoot = value; }
    public Team Team { get => team; set => team = value; }
    public Player FellowPlayer { get => fellowPlayer; set => fellowPlayer = value; }
    public Vector3 InitialPosition { get => initialPosition; set => initialPosition = value; }
    public bool TakeFreeKick { get => takeFreeKick; set => takeFreeKick = value; }
    public bool TakeThrowIn { get => takeThrowIn; set => takeThrowIn = value; }

    void Awake()
    {
        transformBall = GameObject.Find("Ball").transform;
        scriptBall = transformBall.GetComponent<Ball>();
        soundDribble = GameObject.Find("Sound/dribble").GetComponent<AudioSource>();
        soundShoot = GameObject.Find("Sound/shoot").GetComponent<AudioSource>();
        soundSteal = GameObject.Find("Sound/woosh").GetComponent<AudioSource>();
        scriptGame = GameObject.Find("Scripts").GetComponent<Game>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
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
        if (stealDelay > 0) {
            stealDelay -= Time.deltaTime;
        }

        updateTime += Time.deltaTime;
        if (updateTime > 1.0f)
        {
            updateTime = 0f;
        }

        if (StickBallToPlayer)
        {
            transformBall.position = PlayerBallPosition.position;
            transformBall.Rotate(new Vector3(transform.right.x, 0, transform.right.z), scriptBall.Speed * 0.8f, Space.World);
            distanceSinceLastDribble += scriptBall.Speed * Time.deltaTime;
            if (distanceSinceLastDribble > 3)
            {
                soundDribble.Play();
                distanceSinceLastDribble = 0;
            }
        }
        else if (scriptBall.PassDestinationPlayer != fellowPlayer && timeShot == 0 && stealDelay<=0)
        {
            float distanceToPlayer = Vector3.Distance(transformBall.position, PlayerBallPosition.position);
            if (distanceToPlayer < 0.6)
            {
                soundSteal.Play();
                scriptGame.SetPlayerWithBall(this);
                StickBallToPlayer = true;
                scriptBall.TakeBall(this);
            }
        }
        if (starterAssetsInputs!=null && starterAssetsInputs.pass)
        {
            starterAssetsInputs.pass = false;
            Pass();
        }

        if (starterAssetsInputs != null && starterAssetsInputs.shoot)
        {
            starterAssetsInputs.shoot = false;
            Shoot();
        }
        if (timeShot > 0)
        {
            // shoot ball
            if (StickBallToPlayer/* && Time.time - timeShot > 0.2*/)
            {
                soundShoot.Play();
                scriptBall.BallOwnerPlayer = null;
                scriptGame.SetPlayerWithBall(null);
                StickBallToPlayer = false;
                Vector3 shootdirection = transform.forward;
                shootdirection.y += 0.3f;
                rigidbodyBall.AddForce(shootdirection * 20f, ForceMode.Impulse);
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

    public void LooseBall()
    {
        stealDelay = 2.0f;
        StickBallToPlayer = false;
    }

    public void Shoot()
    {
        if (StickBallToPlayer)
        {
            timeShot = Time.time;
            animator.Play("Shoot", LAYER_SHOOT, 0f);
            animator.SetLayerWeight(LAYER_SHOOT, 1f);
        }
    }

    public void Pass()
    {
        if (StickBallToPlayer && scriptBall.PassDestinationPlayer == null)
        {
            transform.LookAt(fellowPlayer.transform.position);
            timeShot = Time.time;
            soundShoot.Play();
            StickBallToPlayer = false;
            animator.Play("Shoot", LAYER_SHOOT, 0f);
            animator.SetLayerWeight(LAYER_SHOOT, 1f);
            playerInput.enabled = false;
            fellowPlayer.Activate();
        }
    }

    public void Activate()
    {
        playerInput.enabled = true;
        scriptBall.Pass(this);
    }

    public void SetPosition(Vector3 position)
    {
        if (Team.IsHuman)
        {
            GetComponent<CharacterController>().enabled = false;
            GetComponent<CharacterController>().transform.position = position;
            GetComponent<CharacterController>().enabled = true;
        }
        else
        {
            transform.position = position;
        }
    }
}
