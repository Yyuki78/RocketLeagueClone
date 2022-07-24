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
    [SerializeField] GameObject BallFallPoint;

    private float respownPoint = 0;//リスポーンする場所 ソロは1つずつ増える

    private bool startCol = false;
    private Quaternion CarRotation;

    private AudioSource _audio;
    [SerializeField] AudioClip _clip1;
    [SerializeField] AudioClip _clip2;

    private GetBoost[] _boost = new GetBoost[34];

    // Start is called before the first frame update
    void Start()
    {
        _ballRigidbody = Ball.GetComponent<Rigidbody>();
        _carRigidbody = MyCar.GetComponent<Rigidbody>();
        _move = MyCar.GetComponent<CarMove3>();
        _audio = GetComponent<AudioSource>();

        int i = 0;
        foreach (GetBoost boostObj in _boost)
        {
            var BoostObj = GameObject.FindGameObjectsWithTag("Boost");
            _boost[i] = BoostObj[i].GetComponent<GetBoost>();
            i++;
        }
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
        Time.timeScale = 0.25f;
        //爆発演出
        var explosion = Instantiate(Explosion, Ball.transform.position, Quaternion.identity);

        Ball.SetActive(false);
        BallFallPoint.SetActive(false);
        this.gameObject.transform.position = Ball.transform.position;

        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 1.0f;
        _audio.PlayOneShot(_clip1);
        yield return new WaitForSeconds(0.4f);
        _audio.PlayOneShot(_clip2);
        yield return new WaitForSeconds(2.2f);
        Destroy(explosion);
        //リセット
        Ball.transform.position = new Vector3(100f, 10.4f, 30f);
        Ball.transform.rotation = new Quaternion(0, 0, 0, 1);
        _ballRigidbody.velocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;
        Ball.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        //リスタート
        isGoalBlue = false;
        isGoalRed = false;

        Ball.SetActive(true);
        BallFallPoint.SetActive(true);
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
                MyCar.transform.position = new Vector3(77.5f, 9.5f, 2.5f);
                CarRotation = Quaternion.Euler(0, 45, 0);
                break;
            case 2:
                MyCar.transform.position = new Vector3(97f, 9.5f, -10f);
                CarRotation = Quaternion.identity;
                break;
            case 3:
                MyCar.transform.position = new Vector3(122.5f, 9.5f, 2.5f);
                CarRotation = Quaternion.Euler(0, -45, 0);
                break;
            case 4:
                MyCar.transform.position = new Vector3(103f, 9.5f, -10f);
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

        yield return new WaitForSeconds(0.1f);
        for(int i = 0; i < 34; i++)
            _boost[i].Reset();

        yield return new WaitForSeconds(0.1f);
        startCol = false;

        yield break;
    }
}
