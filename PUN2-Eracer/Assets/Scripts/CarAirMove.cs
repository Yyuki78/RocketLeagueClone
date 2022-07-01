using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAirMove : MonoBehaviour
{
    private float _inputRoll = 0;
    private float _inputPitch = 0;
    private float _inputYaw = 0;

    private Rigidbody _rigidbody;
    private CarState _state;
    private CarMove3 _move;

    private float BoostVal = 10f;
    [SerializeField] float DividedNum = 1.75f;

    private const float Tr = 36.1f; // 左右回転の回転量
    private const float Dr = -4.5f; // 左右回転の逆回転量

    private const float Tp = 12.1f; // 旋回の回転量
    private const float Dp = -2.8f; // 旋回の逆回転量

    private const float Ty = 9.0f; // 前後回転の回転量
    private const float Dy = -1.9f; // 前後回転の逆回転量

    void Start()
    {
        _rigidbody = GetComponentInParent<Rigidbody>();
        _state = GetComponent<CarState>();
        _move = GetComponent<CarMove3>();
    }

    void Update()
    {
        _inputYaw = GameManager.InputManager.yawInput;
        _inputPitch = GameManager.InputManager.pitchInput;
        _inputRoll = GameManager.InputManager.rollInput;

        //エアロール
        if (GameManager.InputManager.isAirRoll)
        {
            _inputRoll = -_inputYaw;
            _inputYaw = 0;
        }
    }

    private void FixedUpdate()
    {
        if (_state.IsDrive) return;

        //AirBoost
        if (GameManager.InputManager.isBoost && _move.BoostQuantity > 0)
        {
            if (_rigidbody.velocity.magnitude < _move.m_Topspeed2 - 0.1f)
            {
                _rigidbody.AddForce(BoostVal * transform.forward, ForceMode.Acceleration);
            }
            else
            {
                _rigidbody.AddForce(BoostVal * transform.forward / 2f, ForceMode.Acceleration);
            }
        }

        // roll 左右回転
        _rigidbody.AddTorque(Tr * _inputRoll * transform.forward / DividedNum, ForceMode.Acceleration);
        _rigidbody.AddTorque(Dr * transform.InverseTransformDirection(_rigidbody.angularVelocity).z * transform.forward / DividedNum, ForceMode.Acceleration);

        // pitch 旋回
        _rigidbody.AddTorque(Tp * _inputPitch * transform.right / DividedNum, ForceMode.Acceleration);
        _rigidbody.AddTorque(transform.right * (Dp * (1 - Mathf.Abs(_inputPitch)) * transform.InverseTransformDirection(_rigidbody.angularVelocity).x) / DividedNum, ForceMode.Acceleration);

        //yaw 前回転後ろ回転
        _rigidbody.AddTorque(Ty * _inputYaw * transform.up / DividedNum, ForceMode.Acceleration);
        _rigidbody.AddTorque(transform.up * (Dy * (1 - Mathf.Abs(_inputYaw)) * transform.InverseTransformDirection(_rigidbody.angularVelocity).y) / DividedNum, ForceMode.Acceleration);
    }
}
