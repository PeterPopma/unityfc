using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    [SerializeField] private Game scriptGame;

    public void Awake()
    {
        scriptGame = GameObject.Find("Scripts").GetComponent<Game>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Ball>() != null)
        {
            if (name.Equals("GoalDetector1"))
            {
                scriptGame.ScoreGoal(0);
            }
            else
            {
                scriptGame.ScoreGoal(1);
            }
        }
    }
}
