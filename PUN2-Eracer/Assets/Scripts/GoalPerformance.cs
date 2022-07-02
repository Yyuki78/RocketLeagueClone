using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPerformance : MonoBehaviour
{
    public bool isGoalBlue = false;
    public bool isGoalRed = false;

    [SerializeField] GameObject Ball;

    private float respownPoint = 0;//リスポーンする場所 ソロは1つずつ増える



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator GoalEffect()
    {
        Ball.transform.position = new Vector3(100f, 10.4f, 0f);
        Ball.SetActive(false);

        yield return new WaitForSeconds(3f);

        yield break;
    }
}
