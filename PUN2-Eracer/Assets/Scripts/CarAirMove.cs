using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAirMove : MonoBehaviour
{
    private bool isUseDamperTorque = true;

    private float _inputRoll = 0;
    private float _inputPitch = 0;
    private float _inputYaw = 0;

    private Rigidbody _rigidbody;
    private CarState _state;
    private CarMove3 _move;

    private float BoostVal = 10;
    private float DebugF = 2f;

    const float Tr = 36.07956616966136f; // torque coefficient for roll
    const float Tp = 12.14599781908070f; // torque coefficient for pitch
    const float Ty = 8.91962804287785f; // torque coefficient for yaw
    const float Dr = -4.47166302201591f; // drag coefficient for roll
    const float Dp = -2.798194258050845f; // drag coefficient for pitch
    const float Dy = -1.886491900437232f; // drag coefficient for yaw

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
        if (GameManager.InputManager.isBoost && _rigidbody.velocity.magnitude < _move.m_Topspeed2 - 0.1f)
        {
            _rigidbody.AddForce(BoostVal * transform.forward, ForceMode.Acceleration);
        }

        // roll
        _rigidbody.AddTorque(Tr * _inputRoll * transform.forward / DebugF, ForceMode.Acceleration);
        if (isUseDamperTorque) _rigidbody.AddTorque(Dr * transform.InverseTransformDirection(_rigidbody.angularVelocity).z * transform.forward / DebugF, ForceMode.Acceleration);

        // pitch
        _rigidbody.AddTorque(Tp * _inputPitch * transform.right / DebugF, ForceMode.Acceleration);
        if (isUseDamperTorque) _rigidbody.AddTorque(transform.right * (Dp * (1 - Mathf.Abs(_inputPitch)) * transform.InverseTransformDirection(_rigidbody.angularVelocity).x) / DebugF, ForceMode.Acceleration);

        //yaw
        _rigidbody.AddTorque(Ty * _inputYaw * transform.up / DebugF, ForceMode.Acceleration);
        if (isUseDamperTorque) _rigidbody.AddTorque(transform.up * (Dy * (1 - Mathf.Abs(_inputYaw)) * transform.InverseTransformDirection(_rigidbody.angularVelocity).y) / DebugF, ForceMode.Acceleration);
    }
}
