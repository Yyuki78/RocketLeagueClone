using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarJumpMove : MonoBehaviour
{
    //威力調整用
    [SerializeField] float jumpForceVal = 1f;
    [SerializeField] float TorqueVal = 1f;
    //入力量
    private float throttle, steer;

    [SerializeField] private bool isJumping = false;//ジャンプ中か
    [SerializeField] private bool isFliping = false;//フリップ中か
    [SerializeField] private bool _firstJump = false;//一回目のジャンプ
    [SerializeField] private bool _secondJump = false;//二回目のジャンプ=フリップ
    [SerializeField] private bool _jumpTimeOver = false;//空中にいすぎで二回目のジャンプが出来なくなる
    [SerializeField] private bool _canFirstJump = false;//一回目のジャンプが可能か
    [SerializeField] private bool _canSecondJump = false;//二回目のジャンプが可能か
    private int _switchJump = 0;//二回目のジャンプの種類
    private Vector3 _forceAngle;//二回目のジャンプの力
    private Vector3 _jumpAngle;//二回目のジャンプの回転
    private float AirTime = 0f;//空中にいる時間
    private float FlipTime = 0f;//フリップしている時間
    private bool once = false;//一回しか呼ばない用

    private Rigidbody _rigidbody;
    private CarState _state;

    void Start()
    {
        _rigidbody = GetComponentInParent<Rigidbody>();
        _state = GetComponent<CarState>();
    }

    //今回使用する力は全て瞬間なのでUpdateでも問題はない
    private void Update()
    {
        JumpVariables();

        Jump();

        SecondJump();

        JumpBackToTheFeet();
    }

    //フリップの回転だけは加速なのでFixedUpdateで行う
    private void FixedUpdate()
    {
        FlipAnimetion();
    }

    private void JumpVariables()
    {
        //それぞれの変数の変化
        if (_state.IsDrive)
        {
            _firstJump = true;
            isJumping = false;
            _jumpTimeOver = false;
            AirTime = 0;
            once = true;
            _canSecondJump = false;
        }
        else
        {
            _firstJump = false;
            AirTime += Time.deltaTime;
        }

        if (!_firstJump && !_jumpTimeOver && _state.canSecondJump)
        {
            _secondJump = true;
        }
        else
        {
            _secondJump = false;
        }

        if (AirTime > 2.25f)
        {
            _jumpTimeOver = true;
        }
        else
        {
            _jumpTimeOver = false;
        }

        if (AirTime > 0.5f)
        {
            _canFirstJump = false;
        }

        throttle = GameManager.InputManager.throttleInput;
        steer = GameManager.InputManager.steerInput;
    }

    private void Jump()
    {
        //1stジャンプ可能
        if (!GameManager.InputManager.isJump && !_canSecondJump && !_canFirstJump)
        {
            _canFirstJump = true;
        }

        //ジャンプボタンが押されるとジャンプする 1回目
        //本来は1度しか呼ばれないはずなのだが、FixedUpdateとUpdateの都合上
        //ボタンを押し込む量によって呼ばれる回数が変わるので、くしくも元のゲームの再現になった
        if (GameManager.InputManager.isJump && _firstJump && !isJumping && _canFirstJump)
        {
            //VelocityChangeはImpulseの質量無視版
            _rigidbody.AddForceAtPosition(transform.up * 1.0f * jumpForceVal, transform.position, ForceMode.VelocityChange);
            _firstJump = false;
            isJumping = true;

            _state.IsDrive = false;
        }
    }

    //押されたキーで実行内容が変わる
    private void SecondJump()
    {
        //セカンドジャンプ可能
        if (GameManager.InputManager.isJumpUp && _secondJump && !_canSecondJump && once)
        {
            _canSecondJump = true;
        }

        //セカンドジャンプ
        if (GameManager.InputManager.isJump && _secondJump && _canSecondJump && once)
        {
            //0は上,1は前2は後ろ,3は右,4は左,5は右前,6は左前,7は右後ろ,8は左後ろ
            if (throttle > 0.1f)
            {
                if (steer > 0.1f)
                {
                    _switchJump = 5;
                    _forceAngle = Vector3.up / 4 + Vector3.forward / 2 + Vector3.right;
                    _jumpAngle = -Vector3.forward + Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceVal, ForceMode.VelocityChange);
                }
                else if (steer < -0.1f)
                {
                    _switchJump = 6;
                    _forceAngle = Vector3.up / 4 - Vector3.forward / 2 - Vector3.right;
                    _jumpAngle = Vector3.forward + Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceVal, ForceMode.VelocityChange);
                }
                else
                {
                    _switchJump = 1;
                    _forceAngle = Vector3.up / 4 + Vector3.forward * 1.5f;
                    _jumpAngle = Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceVal, ForceMode.VelocityChange);
                }
            }
            else if (throttle < -0.1f)
            {
                if (steer > 0.1f)
                {
                    _switchJump = 7;
                    _forceAngle = Vector3.up / 4 - Vector3.forward + Vector3.right;
                    _jumpAngle = -Vector3.forward - Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceVal, ForceMode.VelocityChange);
                }
                else if (steer < -0.1f)
                {
                    _switchJump = 8;
                    _forceAngle = Vector3.up / 4 - Vector3.forward - Vector3.right;
                    _jumpAngle = Vector3.forward - Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceVal, ForceMode.VelocityChange);
                }
                else
                {
                    _switchJump = 2;
                    _forceAngle = Vector3.up / 2 - Vector3.forward;
                    _jumpAngle = -Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceVal, ForceMode.VelocityChange);
                }
            }
            else
            {
                if (steer > 0.1f)
                {
                    _switchJump = 3;
                    _forceAngle = Vector3.up / 4 + Vector3.right;
                    _jumpAngle = -Vector3.forward;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceVal, ForceMode.VelocityChange);
                }
                else if (steer < -0.1f)
                {
                    _switchJump = 4;
                    _forceAngle = Vector3.up / 4 - Vector3.right;
                    _jumpAngle = Vector3.forward;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceVal, ForceMode.VelocityChange);
                }
                else
                {
                    _switchJump = 0;
                    _jumpAngle = Vector3.zero;
                    _rigidbody.AddForceAtPosition(transform.up * 3f * jumpForceVal, transform.position, ForceMode.VelocityChange);
                }
            }
            _secondJump = false;
            isJumping = true;
            _canSecondJump = false;
            once = false;

            isFliping = true;
        }
    }

    //二回目のジャンプ時にフリップした場合の力学
    private void FlipAnimetion()
    {
        if (isFliping)
        {
            FlipTime += Time.deltaTime;

            _rigidbody.AddRelativeTorque(_jumpAngle * TorqueVal, ForceMode.Acceleration);
        }
        else
        {
            FlipTime = 0f;
        }

        if (FlipTime > 0.5f)
        {
            isFliping = false;
        }
    }

    //BodyGroundDeadStateの時にジャンプボタンでひっくり返る
    private void JumpBackToTheFeet()
    {
        if (_state._states != CarState.CarStates.BodyGroundDead) return;

        if (GameManager.InputManager.isJumpDown)
        {
            _rigidbody.AddForce(Vector3.up * 2f, ForceMode.VelocityChange);
            _jumpAngle = Vector3.forward;
            isFliping = true;
        }
    }
}
