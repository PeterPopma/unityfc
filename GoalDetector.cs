using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Ball>() != null)
        {
            if (name.Equals("GoalDetector1"))
            {
                Game.Instance.ScoreGoal(1);
            }
            else
            {
                Game.Instance.ScoreGoal(0);
            }
        }
    }
}
