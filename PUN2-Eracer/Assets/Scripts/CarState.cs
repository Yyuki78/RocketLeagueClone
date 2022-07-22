using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarState : MonoBehaviour
{
    //ここでは車の状態の管理をする
    //状態としては全てのタイヤが地面についている・幾つかのタイヤが地面についている
    //横向きになっている・空中にいる・ひっくり返っているの4つ
    public enum CarStates
    {
        IsGround,
        SomeWheelsGround,
        BodySideGround,
        Air,
        BodyGroundDead
    }
    public CarStates _states;
    public bool IsDrive;
    public bool SomeWheelHit = false;
    public bool canSecondJump = false;

    //BodyTriggerとWheelTriggerで判別する
    public bool BodyHitting = false;

    private WheelTrigger[] _wtrigger;
    private int WheelHittingNum = 0;

    //Rayでも判別する
    private float _rayLen, _rayOffset = 0.25f;

    private Rigidbody _rigidbody;
    

    // Start is called before the first frame update
    void Start()
    {
        _wtrigger = GetComponentsInChildren<WheelTrigger>();

        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.maxAngularVelocity = 5.5f;

        _rayLen = transform.localScale.x / 2 + _rayOffset;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ChangeState();
        AddDownForce();
    }

    //タイヤと地面の接触判定
    private bool IsRayContact()
    {
        var isHit = Physics.Raycast(transform.position, -transform.up, out var hit, _rayLen);
        return false || isHit;
    }

    private void ChangeState()
    {
        //車輪が地面と接触している数
        for(int i = 0; i < 4; i++)
        {
            if (_wtrigger[i].Hitting)
            {
                WheelHittingNum++;
            }
        }

        //地面にいる
        if (BodyHitting == false && WheelHittingNum >= 3)
        {
            _states = CarStates.IsGround;
        }

        //地面に付きかけor体勢崩しor飛びかけ
        if (BodyHitting == false && WheelHittingNum < 3)
        {
            _states = CarStates.SomeWheelsGround;
        }

        //地面にいる
        if (IsRayContact())
        {
            _states = CarStates.IsGround;
        }

        //車が横倒しになっている
        if (BodyHitting == true)
        {
            if (WheelHittingNum ==1 || WheelHittingNum == 2)
            {
                _states = CarStates.BodySideGround;
            }
        }

        //空中にいる
        if (BodyHitting == false && WheelHittingNum == 0)
        {
            _states = CarStates.Air;
        }

        //車がひっくり返っている
        if (BodyHitting == true && WheelHittingNum == 0)
        {
            _states = CarStates.BodyGroundDead;
        }

        if (_states == CarStates.IsGround/* || _states == CarStates.SomeWheelsGround*/)
        {
            IsDrive = true;
        }
        else
        {
            IsDrive = false;
        }

        if (WheelHittingNum > 0)
        {
            SomeWheelHit = true;
        }
        else
        {
            SomeWheelHit = false;
        }

        if (_states == CarStates.Air || _states == CarStates.SomeWheelsGround)
        {
            canSecondJump = true;
        }
        else
        {
            canSecondJump = false;
        }

        WheelHittingNum = 0;
    }

    //地面と車をくっつける用
    //CarMove側でWheelColliderと一緒にAddForceを使うと無効化されるバグがあるため避難
    private void AddDownForce()
    {
        //ドライブ可能でないor逆さまなら無効
        if (!IsDrive) return;
        if (Vector3.Dot(-Vector3.up, transform.up) > 0.9f) return;
        _rigidbody.AddForce(-transform.up * 100 * 5);
    }
}
