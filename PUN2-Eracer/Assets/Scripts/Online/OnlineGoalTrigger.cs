using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineGoalTrigger : MonoBehaviour
{
    [SerializeField] int GoalMode;//青のゴールなら1,赤のゴールなら2

    [SerializeField] GameObject GameManager;
    private OnlineGameManager _goal;

    // Start is called before the first frame update
    void Start()
    {
        _goal = GameManager.GetComponent<OnlineGameManager>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            Debug.Log("Goal!!!!!!");
            if (GoalMode == 1)
            {
                _goal.isGoalBlue = true;
            }
            else
            {
                _goal.isGoalRed = true;
            }
        }
    }
}
