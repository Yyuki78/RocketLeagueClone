﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPerformance : MonoBehaviour
{
    public bool isGoalBlue = false;
    public bool isGoalRed = false;

    [SerializeField] GameObject Ball;
    private Rigidbody _ballRigidbody;
    [SerializeField] GameObject MyCar;
    private Rigidbody _carRigidbody;
    private CarMove3 _move;
    [SerializeField] GameObject Explosion;

    private float respownPoint = 0;//リスポーンする場所 ソロは1つずつ増える

    private bool startCol = false;
    private Quaternion CarRotation;

    // Start is called before the first frame update
    void Start()
    {
        _ballRigidbody = Ball.GetComponent<Rigidbody>();
        _carRigidbody = MyCar.GetComponent<Rigidbody>();
        _move = MyCar.GetComponent<CarMove3>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGoalBlue || isGoalRed)
        {
            if (!startCol)
            {
                startCol = true;
                StartCoroutine(GoalEffect());
            }
        }
    }

    private IEnumerator GoalEffect()
    {
        //爆発演出
        var explosion = Instantiate(Explosion, Ball.transform.position, Quaternion.identity, this.gameObject.transform);

        //リセット
        Ball.transform.position = new Vector3(100f, 10.4f, 30f);
        Ball.transform.rotation = new Quaternion(0, 0, 0, 1);
        _ballRigidbody.velocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;
        Ball.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        yield return new WaitForSeconds(1.8f);
        yield return new WaitForSeconds(0.8f);
        Destroy(explosion);
        yield return new WaitForSeconds(0.2f);

        //リスタート
        isGoalBlue = false;
        isGoalRed = false;

        Ball.SetActive(true);
        //偶にラグでリセットされてない時があるので二回
        _ballRigidbody.velocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;
        switch (respownPoint)
        {
            case 0:
                MyCar.transform.position = new Vector3(100f, 9.5f, -20f);
                CarRotation = Quaternion.identity;
                break;
            case 1:
                MyCar.transform.position = new Vector3(75f, 9.5f, 0f);
                CarRotation = Quaternion.Euler(0, 45, 0);
                break;
            case 2:
                MyCar.transform.position = new Vector3(95f, 9.5f, -15f);
                CarRotation = Quaternion.identity;
                break;
            case 3:
                MyCar.transform.position = new Vector3(125f, 9.5f, 0f);
                CarRotation = Quaternion.Euler(0, -45, 0);
                break;
            case 4:
                MyCar.transform.position = new Vector3(105f, 9.5f, -15f);
                CarRotation = Quaternion.identity;
                break;
            default:
                Debug.Log("リスタート失敗");
                break;
        }
        respownPoint++;
        if (respownPoint == 5)
        {
            respownPoint = 0;
        }
        MyCar.transform.rotation = CarRotation;
        _move.Respown();

        yield return new WaitForSeconds(0.2f);
        startCol = false;

        yield break;
    }
}
