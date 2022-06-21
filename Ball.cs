using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Player passDestinationPlayer;
    private Player ballOwnerPlayer;
    private float speed;
    private float ballOutOfFieldTimeOut;
    private bool isThrowIn;
    private Vector3 ballOutOfFieldposition;
    Vector2 previousPositon;
    private Game scriptGame;
    private Rigidbody rigidbody;
    private AudioSource soundWhistle;

    public Player PassDestinationPlayer { get => passDestinationPlayer; set => passDestinationPlayer = value; }
    public Player BallOwnerPlayer { get => ballOwnerPlayer; set => ballOwnerPlayer = value; }
    public float Speed { get => speed; }
    public float BallOutOfFieldTimeOut { get => ballOutOfFieldTimeOut; set => ballOutOfFieldTimeOut = value; }
    public Rigidbody Rigidbody { get => rigidbody; set => rigidbody = value; }

    private float timePassedBall;
    private CinemachineVirtualCamera playerFollowCamera;

    private void Awake()
    {
        scriptGame = GameObject.Find("Scripts").GetComponent<Game>();
        playerFollowCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        soundWhistle = GameObject.Find("Sound/whistle").GetComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Pass(Player player)
    {
        timePassedBall = Time.time;
        passDestinationPlayer = player;
    }

    public void TakeBall(Player newBallOwner)
    {
        if (ballOwnerPlayer != null)
        {
            ballOwnerPlayer.LooseBall();
        }
        ballOwnerPlayer = newBallOwner;
    }

    private void BallOutOfField()
    {
        if (scriptGame.PlayerWithBall != null)
        {
            scriptGame.PlayerWithBall.StickBallToPlayer = false;
            scriptGame.SetPlayerWithBall(null);
        }
        transform.position = ballOutOfFieldposition;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        Player player = scriptGame.GetPlayerToTrowIn();
        player.SetPosition(ballOutOfFieldposition);
        if (isThrowIn)
        {
            player.TakeThrowIn = true;
        }
        else
        {
            player.TakeFreeKick = true;
        }
        // move players that are too close
        scriptGame.SetMinimumDistanceOtherPlayers(player);
    }

    private void CheckBallOutOfField()
    {
        // ball out of field
        if (transform.position.z < -25)
        {
            soundWhistle.Play();
            isThrowIn = true;
            ballOutOfFieldTimeOut = 0.5f;
            ballOutOfFieldposition = new Vector3(transform.position.x, 0.772f, -25);
        }
        if (transform.position.z > 25)
        {
            soundWhistle.Play();
            isThrowIn = true;
            ballOutOfFieldTimeOut = 1.0f;
            ballOutOfFieldposition = new Vector3(transform.position.x, 0.772f, 25);
        }

        if (transform.position.x < -52.6f)
        {
            soundWhistle.Play();
            isThrowIn = false;
            ballOutOfFieldTimeOut = 1.0f;
            if (scriptGame.TeamLastTouched == 0)
            {
                // goal kick
                ballOutOfFieldposition = new Vector3(-46.7999992f, 0.772000015f, -4.88999987f);
            }
            else
            {
                // corner
                if (transform.position.z > 0)
                {
                    ballOutOfFieldposition = new Vector3(-52.6f, 0.772f, 25);
                }
                else
                {
                    ballOutOfFieldposition = new Vector3(-52.6f, 0.772f, -25);
                }
            }
        }
        if (transform.position.x > 52.3f)
        {
            soundWhistle.Play();
            isThrowIn = false;
            ballOutOfFieldTimeOut = 1.0f;
            if (scriptGame.TeamLastTouched == 1)
            {
                // goal kick
                ballOutOfFieldposition = new Vector3(46.5800018f, 0.772000015f, 5.28999996f);
            }
            else
            {
                // corner
                if (transform.position.z > 0)
                {
                    ballOutOfFieldposition = new Vector3(52.3f, 0.772f, 25);
                }
                else
                {
                    ballOutOfFieldposition = new Vector3(52.3f, 0.772f, -25);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ballOutOfFieldTimeOut>0)
        {
            ballOutOfFieldTimeOut -= Time.deltaTime;
            if(ballOutOfFieldTimeOut <=0 )
            {
                BallOutOfField();
            }
        }
        else
        {
            CheckBallOutOfField();
        }

        if (Time.deltaTime > 0)
        {
            speed = (float)(Math.Sqrt(Math.Pow(transform.position.x - previousPositon.x, 2) + Math.Pow(transform.position.z - previousPositon.y, 2)) / Time.deltaTime);
        }

        previousPositon.x = transform.position.x;
        previousPositon.y = transform.position.z;

        if (passDestinationPlayer != null)
        {
            if (Time.time - timePassedBall > 0.2 && playerFollowCamera.Follow != passDestinationPlayer.PlayerCameraRoot)
            {
                playerFollowCamera.Follow = passDestinationPlayer.PlayerCameraRoot;
            }

            Vector3 movedirection = passDestinationPlayer.PlayerBallPosition.position - transform.position;
            if (movedirection.magnitude<1f)
            {
                // pass arrived
                transform.position = passDestinationPlayer.PlayerBallPosition.position;
                passDestinationPlayer.StickBallToPlayer = true;
                scriptGame.SetPlayerWithBall(passDestinationPlayer);
                ballOwnerPlayer = passDestinationPlayer;
                passDestinationPlayer = null;
            }
            movedirection.Normalize();
            transform.position += movedirection * 25f * Time.deltaTime;
            transform.Rotate(new Vector3(movedirection.x, 0, movedirection.z), 1000f * Time.deltaTime, Space.Self);
        }
    }

}
