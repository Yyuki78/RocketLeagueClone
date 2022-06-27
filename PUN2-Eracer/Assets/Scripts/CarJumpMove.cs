using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarJumpMove : MonoBehaviour
{
    [Header("Forces")]
    [Range(0.25f, 4)]
    // default 1
    public float jumpForceMultiplier = 1f;
    public int upForce = 3;
    public int upTorque = 50;

    float _jumpTimer = 0;
    bool _isCanFirstJump = false;
    bool _isJumping = false;
    bool _isCanKeepJumping = false;

    private float throttle, steer;

    [SerializeField] private bool isJumping = false;
    [SerializeField] private bool _firstJump = false;
    [SerializeField] private bool _secondJump = false;
    [SerializeField] private bool _jumpTimeOver = false;
    private bool _canSecondJump = false;
    private int _switchJump = 0;
    private Vector3 _forceAngle;
    private Vector3 _jumpAngle;
    [SerializeField] float TorqueVal = 1f;
    private float AirTime = 0f;
    [SerializeField] private bool once = false;

    private IEnumerator reset;

    Rigidbody _rigidbody;
    CarState _state;

    void Start()
    {
        _rigidbody = GetComponentInParent<Rigidbody>();
        _state = GetComponent<CarState>();
        reset = TimeOver();
    }

    //今回使用する力は全て瞬間なのでUpdateでも問題はない
    private void Update()
    {
        JumpVariables();

        Jump();

        SecondJump();
    }

    private void FixedUpdate()
    {
        JumpBackToTheFeet();
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
        }
        else
        {
            _firstJump = false;
            AirTime += Time.deltaTime;
        }

        if (!_firstJump && !_jumpTimeOver)
        {
            _secondJump = true;
        }
        else
        {
            _secondJump = false;
        }

        if (AirTime > 3f)
        {
            _jumpTimeOver = true;
        }
        else
        {
            _jumpTimeOver = false;
        }

        throttle = GameManager.InputManager.throttleInput;
        steer = GameManager.InputManager.steerInput;
    }

    private void Jump()
    {
        /*
        // Do initial jump impulse only once
        // TODO: Currently bugged, should be .isJumpDown for the initial jump impulse.
        // Right now does the whole jump impulse
        if (GameManager.InputManager.isJump && _isCanFirstJump)
        {
            _rigidbody.AddForce(transform.up * 292 / 100 * jumpForceMultiplier, ForceMode.VelocityChange);
            _isCanKeepJumping = true;
            _isCanFirstJump = false;
            _isJumping = true;

            _jumpTimer += Time.fixedDeltaTime;
        }

        // Keep jumping if the jump button is being pressed
        if (GameManager.InputManager.isJump && _isJumping && _isCanKeepJumping && _jumpTimer <= 0.2f)
        {
            _rigidbody.AddForce(transform.up * 1458f / 100 * jumpForceMultiplier, ForceMode.Acceleration);
            _jumpTimer += Time.fixedDeltaTime;
        }

        // If jump button was released we can't start jumping again mid air
        if (GameManager.InputManager.isJumpUp)
            _isCanKeepJumping = false;

        // Reset jump flags when landed
        if (_state.IsDrive)
        {
            // Need a timer, otherwise while jumping we are setting isJumping flag to false right on the next frame 
            if (_jumpTimer >= 0.1f)
                _isJumping = false;

            _jumpTimer = 0;
            _isCanFirstJump = true;
        }
        // Cant start jumping while in the air
        else if (!_state.IsDrive)
            _isCanFirstJump = false;

        */

        //ジャンプボタンが押されるとジャンプする 1回目
        //本来は1度しか呼ばれないはずなのだが、FixedUpdateとUpdateの都合上
        //ボタンを押し込む量によって呼ばれる回数が変わるのでくしくも元のゲームの再現になった
        if (GameManager.InputManager.isJump && _firstJump && !isJumping)
        {
            //VelocityChangeはImpulseの質量無視版
            _rigidbody.AddForceAtPosition(transform.up * 0.8f * jumpForceMultiplier, transform.position, ForceMode.VelocityChange);
            _firstJump = false;
            isJumping = true;

            _state.IsDrive = false;
        }
    }

    //コルーチンをそのまま停止すると再開時に続きから始まるのでリセットする
    private IEnumerator WaitReset()
    {
        //即実行すると次のフレームでリセットされるので少し待つ
        yield return new WaitForSeconds(0.1f);
        if (isJumping) yield break;
        if (!_state.IsDrive) yield break;
        StopCoroutine(reset);
        reset = null;
        reset = TimeOver();
        yield break;
    }

    //1stジャンプから時間が立つと2回目が出来なくなる
    private IEnumerator TimeOver()
    {
        yield return new WaitForSeconds(2);
        if (_state.IsDrive) yield break;
        _jumpTimeOver = true;
        yield break;
    }

    //UpdateのSwitchで実行内容が変わる
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
                    _forceAngle = Vector3.up / 4 + Vector3.forward + Vector3.right;
                    _jumpAngle = -Vector3.forward + Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceMultiplier, ForceMode.VelocityChange);
                    _rigidbody.AddRelativeTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                    _rigidbody.AddTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                }
                else if (steer < -0.1f)
                {
                    _switchJump = 6;
                    _forceAngle = Vector3.up / 4 - Vector3.forward - Vector3.right;
                    _jumpAngle = Vector3.forward + Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceMultiplier, ForceMode.VelocityChange);
                    _rigidbody.AddRelativeTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                    _rigidbody.AddTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                }
                else
                {
                    _switchJump = 1;
                    _forceAngle = Vector3.up / 4 + Vector3.forward;
                    _jumpAngle = Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceMultiplier, ForceMode.VelocityChange);
                    _rigidbody.AddRelativeTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                    _rigidbody.AddTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                }
            }
            else if (throttle < -0.1f)
            {
                if (steer > 0.1f)
                {
                    _switchJump = 7;
                }
                else if (steer < -0.1f)
                {
                    _switchJump = 8;
                }
                else
                {
                    _switchJump = 2;
                    _forceAngle = Vector3.up / 2 - Vector3.forward;
                    _jumpAngle = -Vector3.right;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceMultiplier, ForceMode.VelocityChange);
                    _rigidbody.AddRelativeTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                    _rigidbody.AddTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                }
            }
            else
            {
                if (steer > 0.1f)
                {
                    _switchJump = 3;
                    _forceAngle = Vector3.up / 4 + Vector3.right;
                    _jumpAngle = -Vector3.forward * 200f;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceMultiplier, ForceMode.VelocityChange);
                    _rigidbody.AddRelativeTorque(_jumpAngle * TorqueVal, ForceMode.Impulse);
                    _rigidbody.AddTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                }
                else if (steer < -0.1f)
                {
                    _switchJump = 4;
                    _forceAngle = Vector3.up / 4 - Vector3.right;
                    _jumpAngle = Vector3.forward * 2000000f;
                    _rigidbody.AddRelativeForce(_forceAngle * 3f * jumpForceMultiplier, ForceMode.VelocityChange);
                    _rigidbody.AddRelativeTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                    _rigidbody.AddTorque(_jumpAngle * TorqueVal, ForceMode.VelocityChange);
                }
                else
                {
                    _switchJump = 0;
                    _rigidbody.AddForceAtPosition(transform.up * 3f * jumpForceMultiplier, transform.position, ForceMode.VelocityChange);
                }
            }
            _secondJump = false;
            isJumping = true;
            _canSecondJump = false;
            once = false;
            //SecondJump();
        }

        /*
        //0は上,1は前2は後ろ,3は右,4は左,5は右前,6は左前,7は右後ろ,8は左後ろ
        switch (_switchJump)
        {
            case 0:
                _rigidbody.AddForceAtPosition(transform.up * 3f * jumpForceMultiplier, transform.position, ForceMode.VelocityChange);
                break;
            case 1:
                break;
            default:
                break;
        }
        _secondJump = false;
        _jumpTimeOver = true;
        isJumping = true;
        _canSecondJump = false;*/
    }

    //BodyDeadStateの時にジャンプボタンでひっくり返る
    private void JumpBackToTheFeet()
    {
        if (_state._states != CarState.CarStates.BodyGroundDead) return;

        if (GameManager.InputManager.isJumpDown || Input.GetButtonDown("A"))
        {
            _rigidbody.AddForce(Vector3.up * upForce, ForceMode.VelocityChange);
            _rigidbody.AddTorque(transform.forward * upTorque, ForceMode.VelocityChange);
        }
    }
}
