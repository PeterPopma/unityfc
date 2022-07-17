using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    Slider sliderPowerBar;
    GameObject powerBar;
    public static Game Instance;
    public const float HAVING_BALL_SLOWDOWN_FACTOR = 0.8f;
    public const float PLAYER_Y_POSITION = 0.5f;
    public const float FIELD_BOUNDARY_LOWER_X = -52.6f;
    public const float FIELD_BOUNDARY_UPPER_X = 52.3f;
    public const float FIELD_BOUNDARY_LOWER_Z = -25f;
    public const float FIELD_BOUNDARY_UPPER_Z = 25f;
    [SerializeField] private GameObject playerSpawnPosition1;
    [SerializeField] private GameObject playerSpawnPosition2;
    [SerializeField] private GameObject playerSpawnPosition3;
    [SerializeField] private GameObject playerSpawnPosition4;
    [SerializeField] private GameObject pfPlayer1;
    [SerializeField] private GameObject pfPlayer2;
    [SerializeField] private GameObject pfPlayer3;
    [SerializeField] private GameObject pfPlayer4;
    [SerializeField] private TextMeshProUGUI textScore;
    [SerializeField] private TextMeshProUGUI textGoal;
    [SerializeField] private TextMeshProUGUI textPlayer;
    private Ball scriptBall;
    private AudioSource soundCheer;
    private AudioSource soundWhistle;
    private Player playerLastTouchedBall;
    private Player playerWithBall;
    private Player activeHumanPlayer;
    private Player passDestinationPlayer;
    private CinemachineVirtualCamera playerFollowCamera;
    private int teamWithBall;
    private int teamLastTouched;
    private int teamKickOff;
    private bool waitingForKickOff;
    private float waitingTimeKickOff;
    private float goalTextColorAlpha;
    List<Team> teams = new();
    private Transform[] goals = new Transform[2];
    private Vector3 kickOffPosition = new Vector3(0, PLAYER_Y_POSITION, 0.1f);

    public Player PassDestinationPlayer { get => passDestinationPlayer; set => passDestinationPlayer = value; }
    public Player PlayerWithBall { get => playerWithBall; }
    public int TeamWithBall { get => teamWithBall; set => teamWithBall = value; }
    public int TeamLastTouched { get => teamLastTouched; set => teamLastTouched = value; }
    public bool WaitingForKickOff { get => waitingForKickOff; set => waitingForKickOff = value; }
    public Player ActiveHumanPlayer { get => activeHumanPlayer; set => activeHumanPlayer = value; }
    public List<Team> Teams { get => teams; set => teams = value; }
    public Vector3 KickOffPosition { get => kickOffPosition; set => kickOffPosition = value; }

    public void Awake()
    {
        Instance = this;
        playerFollowCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        soundCheer = GameObject.Find("Sound/cheer").GetComponent<AudioSource>();
        soundWhistle = GameObject.Find("Sound/whistle").GetComponent<AudioSource>();
        scriptBall = GameObject.Find("Ball").GetComponent<Ball>();
        sliderPowerBar = GameObject.Find("Canvas/Panel/PowerBar").GetComponent<Slider>();
        powerBar = GameObject.Find("Canvas/Panel/PowerBar");

        Team newTeam = new Team(0, true);
        teams.Add(newTeam);
        GameObject spawnedPlayer = Instantiate(pfPlayer1, playerSpawnPosition1.transform.position, Quaternion.identity);
        spawnedPlayer.name = "Peter";
        spawnedPlayer.GetComponent<Player>().Number = 0;
        spawnedPlayer.GetComponent<Player>().Team = newTeam;
        spawnedPlayer.GetComponent<Player>().Activate();
        newTeam.Players.Add(spawnedPlayer.GetComponent<Player>());
        playerFollowCamera.Follow = spawnedPlayer.transform.Find("PlayerCameraRoot").transform;
        GameObject spawnedPlayer2 = Instantiate(pfPlayer2, playerSpawnPosition2.transform.position, Quaternion.identity);
        spawnedPlayer2.name = "Mark";
        spawnedPlayer2.GetComponent<Player>().Number = 1;
        spawnedPlayer2.GetComponent<Player>().Team = newTeam;
        spawnedPlayer2.GetComponent<PlayerInput>().enabled = false;
        newTeam.Players.Add(spawnedPlayer2.GetComponent<Player>());

        spawnedPlayer.GetComponent<Player>().FellowPlayer = spawnedPlayer2.GetComponent<Player>();
        spawnedPlayer2.GetComponent<Player>().FellowPlayer = spawnedPlayer.GetComponent<Player>();

        newTeam = new Team(1, false);
        teams.Add(newTeam);
        
        spawnedPlayer = Instantiate(pfPlayer3, playerSpawnPosition3.transform.position, Quaternion.identity);
        spawnedPlayer.name = "Arnoud";
        spawnedPlayer.GetComponent<Player>().Number = 0;
        spawnedPlayer.GetComponent<Player>().Team = newTeam;
        newTeam.Players.Add(spawnedPlayer.GetComponent<Player>());
        spawnedPlayer2 = Instantiate(pfPlayer4, playerSpawnPosition4.transform.position, Quaternion.identity);
        spawnedPlayer2.name = "Thirza";
        spawnedPlayer2.GetComponent<Player>().Number = 1;
        spawnedPlayer2.GetComponent<Player>().Team = newTeam;
        newTeam.Players.Add(spawnedPlayer2.GetComponent<Player>());

        spawnedPlayer.GetComponent<Player>().FellowPlayer = spawnedPlayer2.GetComponent<Player>();
        spawnedPlayer2.GetComponent<Player>().FellowPlayer = spawnedPlayer.GetComponent<Player>();
        
        goals[0] = GameObject.Find("Goal1").transform;
        goals[1] = GameObject.Find("Goal2").transform;
        powerBar.SetActive(false);
    }
    public void Start()
    {
        WaitForKickOff(3.0f);
    }

    public void ResetPlayersAndBall()
    {
        foreach(Team team in teams)
        {
            foreach(Player player in team.Players)
            {
                player.SetPosition(player.InitialPosition);
                player.transform.LookAt(goals[player.Team.Number]);
            }
        }
        // Set player to kick off
        Vector3 position = new(0.5f - teamKickOff, KickOffPosition.y, kickOffPosition.z);
        teams[teamKickOff].Players[0].SetPosition(position);
        scriptBall.BallOutOfFieldTimeOut = 0;
        scriptBall.PutOnCenterSpot();
        scriptBall.Rigidbody.velocity = Vector3.zero;
        scriptBall.Rigidbody.angularVelocity = Vector3.zero;
    }

    public Player PlayerClosestToBall(int teamNumber)
    {
        Player closestPlayer = null;
        float closestDistance = float.MaxValue;
        foreach(Player player in teams[teamNumber].Players)
        {
            float distance = (player.transform.position - scriptBall.transform.position).magnitude;
            if(distance < closestDistance)
            {
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public Player PlayerClosestToLocation(int teamNumber, Vector3 location)
    {
        Player closestPlayer = null;
        float closestDistance = float.MaxValue;
        foreach (Player player in teams[teamNumber].Players)
        {
            float distance = (player.transform.position - location).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public void ScoreGoal(int team)
    {
        teams[team].Score++;
        teamKickOff = OtherTeam(team);
        goalTextColorAlpha = 1;
        soundCheer.Play();
        textScore.text = "Score: " + teams[0].Score + "-" + teams[1].Score;
        playerLastTouchedBall.ScoreGoal();

        WaitForKickOff(5.0f);
    }

    private void WaitForKickOff(float seconds)
    {
        ActiveHumanPlayer.GetComponent<ThirdPersonController>().MoveSpeed = 0;
        ActiveHumanPlayer.GetComponent<ThirdPersonController>().SprintSpeed = 0;
        waitingForKickOff = true;
        waitingTimeKickOff = seconds;
    }

    public void Update()
    {
        if (waitingTimeKickOff > 0)
        {
            waitingTimeKickOff -= Time.deltaTime;
            if (waitingTimeKickOff < 3)
            {
                ResetPlayersAndBall();
            }
            if (waitingTimeKickOff <= 0)
            {
                ActiveHumanPlayer.GetComponent<ThirdPersonController>().MoveSpeed = 8;
                ActiveHumanPlayer.GetComponent<ThirdPersonController>().SprintSpeed = 15;
                waitingForKickOff = false;
                soundWhistle.Play();
            }
        }
        if (goalTextColorAlpha > 0)
        {
            goalTextColorAlpha -= Time.deltaTime;
            textGoal.alpha = goalTextColorAlpha;
            textGoal.fontSize = 350 - (goalTextColorAlpha * 250);
        }

//        PerformSanityChecks();
    }

    private void PerformSanityChecks()
    {
        Player playerWithInputEnabled = null;

        foreach (Team team in teams)
        {
            foreach (Player player in team.Players)
            {
                if (transform.position.y < 0)
                {
                    DisplayErrorMessage("players cannot be lower than the field");
                }
                if (player.GetComponent<PlayerInput>()!=null && player.GetComponent<PlayerInput>().enabled == true)
                {
                    playerWithInputEnabled = player;
                }
            }
        }

        if (playerWithInputEnabled==null)
        {
            DisplayErrorMessage("there must always be a player with input enabled");
        }

        if (ActiveHumanPlayer!=playerWithInputEnabled)
        {
            DisplayErrorMessage("active player must be player with input enabled");
        }

        if (!playerWithInputEnabled.transform.Find("PlayerCameraRoot").transform.parent.name.Equals(playerFollowCamera.Follow.parent.name))
        {
            DisplayErrorMessage("camera must follow player with input enabled");
        }

        if (scriptBall.transform.position.y < 0 && 
            scriptBall.transform.position.x > FIELD_BOUNDARY_LOWER_X &&
            scriptBall.transform.position.x < FIELD_BOUNDARY_UPPER_X && 
            scriptBall.transform.position.z > FIELD_BOUNDARY_LOWER_Z &&
            scriptBall.transform.position.z < FIELD_BOUNDARY_UPPER_Z)
        {
            DisplayErrorMessage("ball in the field cannot be lower than 0");
        }
    }

    private void DisplayErrorMessage(string message)
    {
        Debug.Log("An error has occurred: " + message);
    }

    private int OtherTeam(int team)
    {
        if (team == 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void SetPowerBar(float value)
    {
        powerBar.SetActive(true);
        sliderPowerBar.value = value;
    }

    public void RemovePowerBar()
    {
        powerBar.SetActive(false);
    }

    // Move players that are too close in the direction of the center of the field
    public void SetMinimumDistanceOtherPlayers(Player playerToTakeDistanceFrom)
    {
        foreach (Team team in teams)
        {
            foreach (Player player in team.Players)
            {
                if (player != playerToTakeDistanceFrom)
                {
                    float distance = (player.transform.position-playerToTakeDistanceFrom.transform.position).magnitude;
                    if (distance < 8)
                    {
                        Vector3 moveToDistanceDirection = (Vector3.zero - playerToTakeDistanceFrom.transform.position).normalized * 8;
                        player.SetPosition(new Vector3(player.transform.position.x + moveToDistanceDirection.x,
                                                player.transform.position.y, player.transform.position.z + moveToDistanceDirection.z));
                    }
                }
            }
        }
    }

    public void SetPlayerWithBall(Player player)
    {
        playerWithBall = player;
        PassDestinationPlayer = null;
        if (playerWithBall != null)
        {
            if (player.Team.IsHuman && !player.PlayerInput.enabled)
            {
                playerFollowCamera.Follow = player.PlayerCameraRoot;
                activeHumanPlayer.PlayerInput.enabled = false;
                activeHumanPlayer = player;
                player.PlayerInput.enabled = true;
            }
            scriptBall.PutOnGround();
            player.HasBall = true;
            playerLastTouchedBall = playerWithBall;
            teamLastTouched = teamWithBall = playerWithBall.Team.Number;
            textPlayer.text = playerWithBall.name;
        }
        else
        {
            teamWithBall = -1;
            textPlayer.text = "";
        }
    }

    public Player GetPlayerToThrowIn()
    {
        if (teamLastTouched==0)
        {
            return PlayerClosestToBall(1);
        }
        else
        {
            return PlayerClosestToBall(0);
        }
    }


}
