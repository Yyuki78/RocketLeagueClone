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

    private ChangeCarColor _changeColor;


    public bool isGoalBlue = false;
    public bool isGoalRed = false;

    [SerializeField] GameObject Ball;
    private Rigidbody _ballRigidbody;
    [SerializeField] GameObject Explosion;
    [SerializeField] GameObject BallFallPoint;

    private bool startCol = false;

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
    }

    private void Start()
    {
        _view = myCar.GetComponent<PhotonView>();
        _rpc = myCar.GetComponent<CarRpc>();
        _ballRigidbody = Ball.GetComponent<Rigidbody>();
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
        yield return new WaitForSeconds(0.2f);

        //リスタート
        isGoalBlue = false;
        isGoalRed = false;

        Ball.SetActive(true);
        BallFallPoint.SetActive(true);
        //偶にラグでリセットされてない時があるので二回
        _ballRigidbody.velocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;

        _rpc.Respawn();

        //カウントダウン
        yield return new WaitForSeconds(3.5f);
        startCol = false;

        yield break;
    }
}
