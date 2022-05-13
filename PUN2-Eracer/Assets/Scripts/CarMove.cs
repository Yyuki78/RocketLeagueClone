using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMove : MonoBehaviour
{
    [SerializeField] bool Wheel = false;
    private Rigidbody _rigid;
    private Transform _transform;
    // Start is called before the first frame update
    void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
        _transform = GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        // ▼▼▼移動処理▼▼▼
        if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0)  //  テンキーや3Dスティックの入力（GetAxis）がゼロの時の動作
        {
            //animCon.SetBool("Run", false);  //  Runモーションしない
        }

        else //  テンキーや3Dスティックの入力（GetAxis）がゼロではない時の動作
        {
            var cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;  //  カメラが追従するための動作
            Vector3 direction = cameraForward * Input.GetAxis("Vertical") + Camera.main.transform.right * Input.GetAxis("Horizontal");  //  テンキーや3Dスティックの入力（GetAxis）があるとdirectionに値を返す
                                                                                                                                        //animCon.SetBool("Run", true);  //  Runモーションする

            //MukiWoKaeru(direction);  //  向きを変える動作の処理を実行する（後述）
            //IdoSuru(direction);  //  移動する動作の処理を実行する（後述）
            /*if (Wheel)
            {
                _transform.localEulerAngles += new Vector3(1, 0, 0);
            }*/
            Vector3 force = new Vector3(0.0f, 0.0f, 5f);    // 力を設定
            _rigid.AddForce(force, ForceMode.Force);            // 力を加える

            /*
            Vector3 now = _rigid.position;            // 座標を取得
            now += new Vector3(0.0f, 0.0f, 0.5f);  // 前に少しずつ移動するように加算
            _rigid.transform.localPosition = now; // 値を設定*/
        }

        if (Input.GetKey("[4]") || Input.GetKey(KeyCode.A))
        {
            _transform.localEulerAngles += new Vector3(0, -1, 0);
        }

        if (Input.GetKey("[4]") || Input.GetKey(KeyCode.S))
        {
            Vector3 force = new Vector3(0.0f, 0.0f, -5f);    // 力を設定
            _rigid.AddForce(force, ForceMode.Force);            // 力を加える
        }

        if (Input.GetKey("[4]") || Input.GetKey(KeyCode.D))
        {
            _transform.localEulerAngles += new Vector3(0, 1, 0);
        }

        if (Input.GetKeyDown("[1]") || Input.GetKeyDown(KeyCode.Space)){
            Vector3 force = new Vector3(0.0f, 500f, 0f);    // 力を設定
            _rigid.AddForce(force, ForceMode.Force);            // 力を加える
        }
    }
}
