using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
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
    private float goalTextColorAlpha;
    private CinemachineVirtualCamera playerFollowCamera;
    private Player playerWithBall;
    private int teamWithBall;
    private int teamLastTouched;
    private int teamKickOff;
    List<Team> teams = new List<Team>();
    private Transform[] goals = new Transform[2];

    public Player PlayerWithBall { get => playerWithBall; }
    public int TeamWithBall { get => teamWithBall; set => teamWithBall = value; }
    public int TeamLastTouched { get => teamLastTouched; set => teamLastTouched = value; }

    public void Awake()
    {
        playerFollowCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        soundCheer = GameObject.Find("Sound/cheer").GetComponent<AudioSource>();
        scriptBall = GameObject.Find("Ball").GetComponent<Ball>();

        Team newTeam = new Team(0, true);
        teams.Add(newTeam);
        GameObject spawnedPlayer = Instantiate(pfPlayer1, playerSpawnPosition1.transform.position, Quaternion.identity);
        spawnedPlayer.name = "Peter";
        spawnedPlayer.GetComponent<Player>().Team = newTeam;
        spawnedPlayer.GetComponent<PlayerInput>().enabled = true;
        newTeam.Players.Add(spawnedPlayer.GetComponent<Player>());
        playerFollowCamera.Follow = spawnedPlayer.transform.Find("PlayerCameraRoot").transform;
        GameObject spawnedPlayer2 = Instantiate(pfPlayer2, playerSpawnPosition2.transform.position, Quaternion.identity);
        spawnedPlayer2.name = "Mark";
        spawnedPlayer2.GetComponent<Player>().Team = newTeam;
        spawnedPlayer2.GetComponent<PlayerInput>().enabled = false;
        newTeam.Players.Add(spawnedPlayer2.GetComponent<Player>());

        spawnedPlayer.GetComponent<Player>().FellowPlayer = spawnedPlayer2.GetComponent<Player>();
        spawnedPlayer2.GetComponent<Player>().FellowPlayer = spawnedPlayer.GetComponent<Player>();

        newTeam = new Team(1, false);
        teams.Add(newTeam);
        spawnedPlayer = Instantiate(pfPlayer3, playerSpawnPosition3.transform.position, Quaternion.identity);
        spawnedPlayer.name = "Thirza";
        spawnedPlayer.GetComponent<Player>().Team = newTeam;
        newTeam.Players.Add(spawnedPlayer.GetComponent<Player>());
        spawnedPlayer2 = Instantiate(pfPlayer4, playerSpawnPosition4.transform.position, Quaternion.identity);
        spawnedPlayer2.name = "Arnoud";
        spawnedPlayer2.GetComponent<Player>().Team = newTeam;
        newTeam.Players.Add(spawnedPlayer2.GetComponent<Player>());

        spawnedPlayer.GetComponent<Player>().FellowPlayer = spawnedPlayer2.GetComponent<Player>();
        spawnedPlayer2.GetComponent<Player>().FellowPlayer = spawnedPlayer.GetComponent<Player>();

        goals[0] = GameObject.Find("Goal1").transform;
        goals[1] = GameObject.Find("Goal2").transform;
    }
    public void Start()
    {
        ResetPlayersAndBall();
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
        teams[teamKickOff].Players[0].SetPosition(new Vector3((float)(0.5 - teamKickOff), 0.5f, 0.1f));
        scriptBall.BallOutOfFieldTimeOut = 0;
        scriptBall.transform.position = new Vector3(-0.100000001f, 0.772000015f, -0.0489999987f);
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

    public void ScoreGoal(int team)
    {
        teams[team].Score++;
        teamKickOff = OtherTeam(team);
        ResetPlayersAndBall();
        goalTextColorAlpha = 1;
        soundCheer.Play();
        textScore.text = "Score: " + teams[0].Score + "-" + teams[1].Score;
    }

    public void Update()
    {
        if (goalTextColorAlpha > 0)
        {
            goalTextColorAlpha -= Time.deltaTime;
            textGoal.alpha = goalTextColorAlpha;
            textGoal.fontSize = 350 - (goalTextColorAlpha * 250);
        }
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

    // Move players that are too close in the direction of the center of the field
    public void SetMinimumDistanceOtherPlayers(Player currentPlayer)
    {
        foreach (Team team in teams)
        {
            foreach (Player player in team.Players)
            {
                if (player != currentPlayer)
                {
                    float distance = new Vector2(player.transform.position.x-currentPlayer.transform.position.x, player.transform.position.z - currentPlayer.transform.position.z).magnitude;
                    if (distance < 8)
                    {
                        player.SetPosition(player.transform.position + (Vector3.zero - currentPlayer.transform.position).normalized * 8);
                    }
                }
            }
        }
    }

    public void SetPlayerWithBall(Player player)
    {
        playerWithBall = player;
        if (playerWithBall != null)
        {
            textPlayer.text = playerWithBall.name;
            teamLastTouched = teamWithBall = playerWithBall.Team.Number;
        }
        else
        {
            teamWithBall = -1;
            textPlayer.text = "";
        }
    }

    public Player GetPlayerToTrowIn()
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
