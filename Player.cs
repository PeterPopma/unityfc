using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textScore;
    [SerializeField] private TextMeshProUGUI textGoal;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    private Ball ballAttachedToPlayer;
    private float timeShot;
    private const int LAYER_SHOOT = 1;
    private int myScore, otherScore;
    private AudioSource soundDribble;
    private AudioSource soundShoot;
    private AudioSource soundCheer;
    private CharacterController characterController;
    private float distanceSinceLastDribble;
    private float goalTextColorAlpha;

    public Ball BallAttachedToPlayer { get => ballAttachedToPlayer; set => ballAttachedToPlayer = value; }

    // Start is called before the first frame update
    void Start()
    {
        soundDribble = GameObject.Find("Sound/dribble").GetComponent<AudioSource>();
        soundShoot = GameObject.Find("Sound/shoot").GetComponent<AudioSource>();
        soundCheer = GameObject.Find("Sound/cheer").GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float speed = new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude;
        if (starterAssetsInputs.shoot)
        {
            starterAssetsInputs.shoot = false;
            timeShot = Time.time;
            animator.Play("Shoot", LAYER_SHOOT, 0f);
            animator.SetLayerWeight(LAYER_SHOOT, 1f);
        }
        if (timeShot > 0)
        {
            // shoot ball
            if( ballAttachedToPlayer != null && Time.time - timeShot > 0.2)
            {
                soundShoot.Play();
                ballAttachedToPlayer.StickToPlayer = false;
                Rigidbody rigidbody = ballAttachedToPlayer.transform.gameObject.GetComponent<Rigidbody>();
                Vector3 shootdirection = transform.forward;
                shootdirection.y += 0.3f;
                rigidbody.AddForce(shootdirection * 20f, ForceMode.Impulse);
                ballAttachedToPlayer = null;
            }

            // finished kicking animation
            if(Time.time - timeShot > 0.5)
            {
                timeShot = 0;
            }
        }
        else
        {
            animator.SetLayerWeight(LAYER_SHOOT, Mathf.Lerp(animator.GetLayerWeight(LAYER_SHOOT), 0f, Time.deltaTime * 10f));
        }

        if (goalTextColorAlpha>0)
        {
            goalTextColorAlpha -= Time.deltaTime;
            textGoal.alpha = goalTextColorAlpha;
            textGoal.fontSize = 200 - (goalTextColorAlpha * 120);
        }

        if (ballAttachedToPlayer!=null)
        {
            distanceSinceLastDribble += speed * Time.deltaTime;
            if (distanceSinceLastDribble > 3)
            {
                soundDribble.Play();
                distanceSinceLastDribble = 0;
            }
        }
    }

    public void IncreaseMyScore()
    {
        myScore++;
        UpdateScore();
    }
    public void IncreaseOtherScore()
    {
        otherScore++;
        UpdateScore();
    }

    private void UpdateScore()
    {
        soundCheer.Play();
        textScore.text = "Score: " + myScore + "-" + otherScore;
    }
}
