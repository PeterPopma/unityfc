using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private Transform transformPlayer;
    private bool stickToPlayer;
    private Transform playerBallPosition;
    private Player scriptPlayer;
    private Vector2 previousLocation;
    private float speed;

    public bool StickToPlayer { get => stickToPlayer; set => stickToPlayer = value; }


    // Start is called before the first frame update
    void Start()
    {
        scriptPlayer = transformPlayer.GetComponent<Player>();
        playerBallPosition = transformPlayer.Find("BallPosition");
    }

    // Update is called once per frame
    void Update()
    {
        if (!stickToPlayer)
        {
            float distanceToPlayer = Vector3.Distance(transformPlayer.position, transform.position);
            if (distanceToPlayer < 0.5)
            {
                stickToPlayer = true;
                scriptPlayer.BallAttachedToPlayer = this;
            }
        }
        else
        {
            Vector2 currentLocation = new Vector2(transform.position.x, transform.position.z);
            speed = Vector2.Distance(currentLocation, previousLocation) / Time.deltaTime;
            transform.position = playerBallPosition.position;
            transform.Rotate(new Vector3(transformPlayer.right.x, 0, transformPlayer.right.z), speed, Space.World);
            previousLocation = currentLocation;
        }

        // respawn ball if fallen
        if (transform.position.y < -2)
        {
            transform.position = new Vector3(-0.178f, 0.772f, -0.054f);
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }

}
