using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    private Vector3 pos;
    private Quaternion rotate;

    private bool once = true;

    private GameObject myCar;
    private PhotonView _view;
    private CarRpc _rpc;
    private CarMove3 _move;

    private ChangeCarColor _changeColor;


    public bool isGoalBlue = false;
    public bool isGoalRed = false;

    [SerializeField] GameObject Ball;
    private Rigidbody _ballRigidbody;
    [SerializeField] GameObject Explosion;
    [SerializeField] GameObject BallFallPoint;

    private bool startCol = false;

    public bool isCountdown = false;

    
    private float elapsedTime;
    private float stoppingTime;
    private float StopTime = 0f;
    private float DisplayTime;

    public float DisplayMinutes;
    public float DisplaySeconds;

    // Start is called before the first frame update
    void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pos = new Vector3(77.5f, 9.5f, 2.5f);
            rotate = Quaternion.Euler(0, 45, 0);
            PhotonNetwork.NickName = "1P";
        }
        else
        {
            pos = new Vector3(122.5f, 9.5f, 57.5f);
            rotate = Quaternion.Euler(0, 180, 0);
            PhotonNetwork.NickName = "2P";
        }
        myCar = PhotonNetwork.Instantiate("RocketCar1v1", pos, rotate);

        /*
        // ルームを作成したプレイヤーは、現在のサーバー時刻をゲームの開始時刻に設定する
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetStartTime(PhotonNetwork.ServerTimestamp);
        }*/
    }

    private void Start()
    {
        _view = myCar.GetComponent<PhotonView>();
        _rpc = myCar.GetComponent<CarRpc>();
        _move = myCar.GetComponent<CarMove3>();
        _ballRigidbody = Ball.GetComponent<Rigidbody>();
        StartCoroutine(StartCountdown());
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

            if (!PhotonNetwork.CurrentRoom.TryGetStopTime(out int timestamp)) { return; }
            stoppingTime = Mathf.Max(0f, unchecked(PhotonNetwork.ServerTimestamp - timestamp) / 1000f);
        }
        else
        {
            // まだルームに参加していない場合は更新しない
            if (!PhotonNetwork.InRoom) { return; }
            // まだゲームの開始時刻が設定されていない場合は更新しない
            if (!PhotonNetwork.CurrentRoom.TryGetStartTime(out int timestamp)) { return; }

            // ゲームの経過時間を求める
            elapsedTime = Mathf.Max(0f, unchecked(PhotonNetwork.ServerTimestamp - timestamp) / 1000f);
            DisplayTime = 300 + StopTime - elapsedTime;
            DisplayMinutes = (int)DisplayTime / 60;
            DisplaySeconds = (int)DisplayTime % 60;

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.SetStopTime(PhotonNetwork.ServerTimestamp);
            }
        }
    }

    private IEnumerator StartCountdown()
    {
        DisplayMinutes = 5;
        DisplaySeconds = 0;
        yield return new WaitForSeconds(1f);
        isCountdown = true;
        yield return new WaitForSeconds(3.3f);
        _move.isMoving = true;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetStartTime(PhotonNetwork.ServerTimestamp);
        }

        yield break;
    }

        private IEnumerator GoalEffect()
    {
        Time.timeScale = 0.25f;
        //爆発演出
        var explosion = Instantiate(Explosion, Ball.transform.position, Quaternion.identity, this.gameObject.transform);

        Ball.SetActive(false);
        BallFallPoint.SetActive(false);

        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 1.0f;
        yield return new WaitForSeconds(1.8f);
        yield return new WaitForSeconds(0.8f);
        Destroy(explosion);
        //リセット
        Ball.transform.position = new Vector3(100f, 10.4f, 30f);
        Ball.transform.rotation = new Quaternion(0, 0, 0, 1);
        _ballRigidbody.velocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;
        Ball.SetActive(false);
        yield return new WaitForSeconds(0.1f);

        Ball.SetActive(true);
        BallFallPoint.SetActive(true);
        //偶にラグでリセットされてない時があるので二回
        _ballRigidbody.velocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;

        _move.isMoving = false;
        _move.Respown();
        yield return new WaitForSeconds(0.1f);

        _move.isMoving = false;
        _move.Respown();
        _rpc.Respawn();
        yield return new WaitForSeconds(0.1f);
        _move.Respown();

        isCountdown = true;

        //カウントダウン
        yield return new WaitForSeconds(3.3f);
        startCol = false;

        //リスタート
        isGoalBlue = false;
        isGoalRed = false;

        yield return new WaitForSeconds(0.05f);

        isCountdown = false;

        StopTime = StopTime + stoppingTime;
        yield return new WaitForSeconds(0.05f);

        yield break;
    }
}
