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

    private AudioSource _audio;
    [SerializeField] AudioClip _clip1;
    [SerializeField] AudioClip _clip2;
    [SerializeField] AudioClip _clip3;

    private bool startCol = false;

    public bool isCountdown = false;

    
    private float elapsedTime;
    private float stoppingTime;
    private float StopTime = 0f;
    private float DisplayTime;

    public float DisplayMinutes;
    public float DisplaySeconds;

    [SerializeField] GameObject ResultPanel;

    private GetBoost[] _boost = new GetBoost[34];

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
        _audio = GetComponent<AudioSource>();
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

            PhotonNetwork.CurrentRoom.SetStopTime(PhotonNetwork.ServerTimestamp);
        }
        if (DisplayTime <= 0)
        {
            //ゲーム終了
            Time.timeScale = 0f;
            ResultPanel.SetActive(true);
        }
    }

    private IEnumerator StartCountdown()
    {
        DisplayMinutes = 5;
        DisplaySeconds = 0;
        yield return new WaitForSeconds(0.5f);
        int i = 0;
        foreach (GetBoost boostObj in _boost)
        {
            var BoostObj = GameObject.FindGameObjectsWithTag("Boost");
            _boost[i] = BoostObj[i].GetComponent<GetBoost>();
            i++;
        }
        yield return new WaitForSeconds(0.5f);
        isCountdown = true;

        yield return new WaitForSeconds(0.3f);
        this.gameObject.transform.position = Ball.transform.position;
        _audio.volume = 0.1f;
        _audio.PlayOneShot(_clip3);
        yield return new WaitForSeconds(3.0f);
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
        var explosion = Instantiate(Explosion, Ball.transform.position, Quaternion.identity);

        Ball.SetActive(false);
        BallFallPoint.SetActive(false);
        this.gameObject.transform.position = Ball.transform.position;

        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 1.0f;
        _audio.volume = 0.3f;
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
        yield return new WaitForSeconds(0.1f);

        
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

        Ball.SetActive(true);
        BallFallPoint.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 34; i++)
            _boost[i].Reset();

        //カウントダウン
        yield return new WaitForSeconds(0.2f);
        this.gameObject.transform.position = Ball.transform.position;
        _audio.volume = 0.1f;
        _audio.PlayOneShot(_clip3);
        yield return new WaitForSeconds(3.0f);

        StopTime = StopTime + stoppingTime;

        yield return new WaitForSeconds(0.05f);

        isCountdown = false;

        //リスタート
        isGoalBlue = false;
        isGoalRed = false;

        yield return new WaitForSeconds(0.05f);
        startCol = false;

        yield break;
    }
}
