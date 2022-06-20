using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMove2 : MonoBehaviour
{
    [SerializeField] float turnRadiusCoefficient = 50;
    public float currentSteerAngle;
    
    [SerializeField] float driftTime = 3;
    public float currentWheelSideFriction = 10;
    [SerializeField] float wheelSideFriction = 8;
    [SerializeField] float wheelSideFrictionDrift = 0.5f;

    public float forwardSpeed = 0, forwardSpeedSign = 0, forwardSpeedAbs = 0;

    //ブースト
    private float BoostForceMultiplier = 1f;
    const float BoostForce = 991*3.25f / 100;//3倍
    private const float MaxSpeedBoost = 2300 / 100;//1倍

    Rigidbody _rigidbody;
    CarState _state;
    WheelForce[] _wheelForce;
    [SerializeField] Transform _rotateObj;

    void Start()
    {
        _rigidbody = GetComponentInParent<Rigidbody>();
        _state = GetComponent<CarState>();
        _wheelForce = GetComponentsInChildren<WheelForce>();
    }

    private void FixedUpdate()
    {
        //重力をかけるかかけないか
        //Gravity();

        //Steering();

        //ブースト
        Boosting();

        //車を地面にくっつくける用
        DownForce();

        if (_state.SomeWheelHit && !_state.IsDrive)
        {
            DownForce2();
        }

        if (Vector3.Dot(Vector3.up, transform.up) < 0.98f && transform.position.y < 3.75f && _state.SomeWheelHit && !_state.IsDrive)
        {
            DownForce3();
        }

        UpdateCarVariables();

        SetDriftFriction();

        var forwardAcceleration = CalcForwardForce(GameManager.InputManager.throttleInput);
        ApplyWheelForwardForce(forwardAcceleration);
        //ApplyForwardForce(forwardAcceleration);

        currentSteerAngle = CalculateSteerAngle();

        ApplyWheelRotation(currentSteerAngle);

    }

    private void Gravity()
    {
        if (GameManager.InputManager.throttleInput > 0 || GameManager.InputManager.isBoost)
        {
            if (_state.IsDrive)
            {
                _rigidbody.useGravity = false;
            }
            else
            {
                _rigidbody.useGravity = true;
            }
        }
        else
        {
            _rigidbody.useGravity = true;
        }
    }

    /*void Steering()
    {
        _rigidbody.AddTorque(transform.up * (Input.GetAxis("Horizontal") * _naiveRotationForce), ForceMode.Acceleration);
        _rigidbody.AddTorque(transform.up * (_naiveRotationDampeningForce * (1 - Mathf.Abs(Input.GetAxis("Horizontal"))) *
                                      transform.InverseTransformDirection(_rigidbody.angularVelocity).y), ForceMode.Acceleration);
    }*/

    /// ステアリング操作.
    void Steering()
    {
        if (Input.GetKey(KeyCode.A) == true)
        {
            _rigidbody.AddTorque(-transform.up * 3000f, ForceMode.Force);
        }

        if (Input.GetKey(KeyCode.D) == true)
        {
            _rigidbody.AddTorque(transform.up * 3000f, ForceMode.Force);
        }
    }

    private void DownForce3()
    {
        if (this.transform.position.y > 43f) return;
        if (_state._states == CarState.CarStates.Air) return;
        _rigidbody.AddForce(-transform.up * 5 * 5*5*2, ForceMode.Acceleration);
    }

    //車の下方向への重力　使い方↓
    //一個のタイヤが地面と接触している時に即座に体勢を立て直す
    private void DownForce2()
    {
        if (this.transform.position.y > 43f) return;
        _rigidbody.AddForce(-transform.up * 5 * 5, ForceMode.Acceleration);
    }

    //車の下方向への重力
    private void DownForce()
    {
        if (this.transform.position.y > 43f) return;
        var pos = transform.position;
        pos.y = transform.position.y - 0.2f;
        if (_state.IsDrive)
            _rigidbody.AddForceAtPosition(-transform.up * 4 / 5, pos, ForceMode.Acceleration);

        //if (GameManager.InputManager.isBoost && forwardSpeed < MaxSpeedBoost)
            //_rigidbody.AddForce(-transform.up * _rigidbody.velocity.magnitude * 4 / 5, ForceMode.Acceleration);

    }

    //車のスピード量の変数まとめ
    private void UpdateCarVariables()
    {
        forwardSpeed = Vector3.Dot(_rigidbody.velocity, transform.forward);
        forwardSpeed = (float)System.Math.Round(forwardSpeed, 2);
        forwardSpeedAbs = Mathf.Abs(forwardSpeed);
        forwardSpeedSign = Mathf.Sign(forwardSpeed);
    }

    //ドリフト
    private void SetDriftFriction()
    {
        // Sliding / drifting, lowers the wheel side friction when drifting
        var currentDriftDrag = GameManager.InputManager.isDrift ? wheelSideFrictionDrift : wheelSideFriction;
        currentWheelSideFriction = Mathf.MoveTowards(currentWheelSideFriction, currentDriftDrag, Time.deltaTime * driftTime);
        if (GameManager.InputManager.isDrift && GameManager.InputManager.isBoost)
        {
            //turnRadiusCoefficient = 100;
            //_rigidbody.AddTorque(Mathf.Sign(currentSteerAngle) *transform.up * 10000f, ForceMode.Force);
            //transform.Rotate(Vector3.right * currentSteerAngle/10);
        }
        else
        {
            //turnRadiusCoefficient = 50;
        }
    }
    
    //タイヤに渡して移動する
    private void ApplyWheelForwardForce(float forwardAcceleration)
    {
        /*
        if (_state.IsDrive && GameManager.InputManager.isBoost == false)
        {
            //wheel.ApplyForwardForce(forwardAcceleration / 4);
            _rigidbody.AddForce(forwardAcceleration * 4*4 * transform.forward, ForceMode.Acceleration);
            //16倍
        }*/

        if (_state.SomeWheelHit && !_state.IsDrive) return;

        //それぞれのタイヤに渡して移動させる
        foreach (var wheel in _wheelForce)
        {
            //TODO: Func. call like this below OR Wheel class fetches data from this class?
            // Also probably should be an interface to a concrete implementation. Same for the NaiveGroundControl below.
            if (_state.IsDrive && GameManager.InputManager.isBoost == false)
            {
                //wheel.ApplyForwardForce(forwardAcceleration / 4);
                wheel.ApplyForwardForce(forwardAcceleration*1.5f);
                //16倍
            }
        }
    }
    /*
    //前後に移動する
    public void ApplyForwardForce(float force)
    {
        if (_state.IsDrive && GameManager.InputManager.isBoost == false)
        {
            _rigidbody.AddForce(force * 4 * transform.forward, ForceMode.Acceleration);

            // Kill velocity to 0 for small car velocities
            if (force == 0 && forwardSpeedAbs < 0.1)
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, 0);
        }
    }*/

    //タイヤ自体を回して方向転換する
    private void ApplyWheelRotation(float steerAngle)
    {
        if (!_state.IsDrive) return;

        if (steerAngle < -1)
        {
            _rigidbody.AddTorque(transform.up * -10f, ForceMode.Acceleration);
            //_rotateObj.localEulerAngles += new Vector3(0, -1f, 0);
        }
        else if(steerAngle > 1)
        {
            _rigidbody.AddTorque(transform.up * 10f, ForceMode.Acceleration);
            //_rotateObj.localEulerAngles += new Vector3(0, 1f, 0);
        }

        // Apply steer angle to each wheel
        foreach (var wheel in _wheelForce)
        {
            //TODO: Func. call like this below OR Wheel class fetches data from this class?
            // Also probably should be an interface to a concrete implementation. Same for the NaiveGroundControl below.
            wheel.RotateWheels(steerAngle);
        }
    }

    //ブースト機能
    void Boosting()
    {
        if (GameManager.InputManager.isBoost && forwardSpeed < MaxSpeedBoost)
        {
            if (_state.IsDrive)
            {
                //var forwa= CalcForwardForce(GameManager.InputManager.throttleInput);
                //_rigidbody.AddForce(forwa * 4 * transform.forward, ForceMode.Acceleration);
                _rigidbody.AddForce(BoostForce * BoostForceMultiplier * transform.forward, ForceMode.Acceleration);
            }
            else
            {
                _rigidbody.AddForce(BoostForce * BoostForceMultiplier * transform.forward/2, ForceMode.Acceleration);
            }
        }
    }

    //入力量の計算
    private float CalcForwardForce(float throttleInput)
    {
        // Throttle
        float forwardAcceleration = 0;

        if (GameManager.InputManager.isBoost)
            forwardAcceleration = GetForwardAcceleration(forwardSpeedAbs);
        else
            forwardAcceleration = throttleInput * GetForwardAcceleration(forwardSpeedAbs);

        // 止まる
        if (forwardSpeedSign != Mathf.Sign(throttleInput) && throttleInput != 0)
            forwardAcceleration += -1 * forwardSpeedSign * 35;
        return forwardAcceleration;
    }

    private float CalculateForwardForce(float input, float speed)
    {
        return input * GetForwardAcceleration(forwardSpeedAbs);
    }

    private float CalculateSteerAngle()
    {
        var curvature = 1 / GetTurnRadius(forwardSpeed);
        //var curvature = GetTurnRadius(forwardSpeed);
        return GameManager.InputManager.steerInput * curvature * turnRadiusCoefficient;
    }

    static float GetForwardAcceleration(float speed)
    {
        // Replicates acceleration curve from RL, depends on current car forward velocity
        speed = Mathf.Abs(speed);
        float throttle = 0;
        /*
        //全部2倍
        if (speed > (1410 * 2 / 100))
            throttle = 0;
        else if (speed > (1400 * 2 / 100))
            throttle = CarMove2.Scale(14, 14.1f, 1.6f, 0, speed);
        else if (speed <= (1400 * 2 / 100))
            throttle = CarMove2.Scale(0, 14, 16, 1.6f, speed);
        */
        if (speed > (1410 / 100))
            throttle = 0;
        else if (speed > (1400 / 100))
            throttle = CarMove2.Scale(14, 14.1f, 1.6f, 0, speed);
        else if (speed <= (1400 / 100))
            throttle = CarMove2.Scale(0, 14, 16, 1.6f, speed);

        return throttle;
    }

    
    static float GetTurnRadius(float speed)
    {
        float forwardSpeed = Mathf.Abs(speed);
        float turnRadius = 0;

        float DebugRad = 1f;
        /*
        var curvature = CarMove2.Scale(0, 5 * DebugRad, 0.00225f * DebugRad, 0.0015f * DebugRad, forwardSpeed);

        if (forwardSpeed >= 500 / 100)
            curvature = CarMove2.Scale(5 * DebugRad, 10 * DebugRad, 0.00225f * DebugRad, 0.0015f * DebugRad, forwardSpeed);

        if (forwardSpeed >= 1000 / 100)
            curvature = CarMove2.Scale(10 * DebugRad, 15 * DebugRad, 0.00225f * DebugRad, 0.00175f * DebugRad, forwardSpeed);

        if (forwardSpeed >= 1500 / 100)
            curvature = CarMove2.Scale(15 * DebugRad, 20 * DebugRad, 0.00175f * DebugRad, 0.001f * DebugRad, forwardSpeed);

        if (forwardSpeed >= 2000 / 100)
            curvature = CarMove2.Scale(20 * DebugRad, 23 * DebugRad, 0.00125f * DebugRad, 0.001f * DebugRad, forwardSpeed);
        */

        var curvature = CarMove2.Scale(0, 5 * DebugRad, 0.0069f * DebugRad, 0.00398f * DebugRad, forwardSpeed);

        if (forwardSpeed >= 500 * 1 / 100)
            curvature = CarMove2.Scale(5 * DebugRad, 10 * DebugRad, 0.00398f * DebugRad, 0.00235f * DebugRad, forwardSpeed);

        if (forwardSpeed >= 1000 * 1 / 100)
            curvature = CarMove2.Scale(10 * DebugRad, 15 * DebugRad, 0.00215f * DebugRad, 0.001375f * DebugRad, forwardSpeed);
        if (forwardSpeed >= 1500 * 1 / 100)
            curvature = CarMove2.Scale(15 * DebugRad, 17.5f * DebugRad, 0.001375f * DebugRad, 0.0011f * DebugRad, forwardSpeed);

        if (forwardSpeed >= 1750 / 100)
            curvature = CarMove2.Scale(17.5f * DebugRad, 23 * DebugRad, 0.0011f * DebugRad, 0.00088f * DebugRad, forwardSpeed);

        //if (forwardSpeed >= 1750 * 2 / 100)
        //curvature = CarMove2.Scale(17.5f * DebugRad, 23 * DebugRad, 0.0011f * DebugRad, 0.00088f * DebugRad, forwardSpeed);

        turnRadius = 1 / (curvature * 100);

        /*
        if (forwardSpeed < 10)
            turnRadius = 34.5f / (50 * 1);

        if (forwardSpeed >= 10)
            turnRadius = 34.5f / (50 * 1);

        if (forwardSpeed >= 20)
            turnRadius = 34.5f / (50 * 1.5f);

        if (forwardSpeed >= 30)
            turnRadius = 34.5f / (50 * 2);

        if (forwardSpeed >= 40)
            turnRadius = 34.5f / (50 * 2.5f);*/

        return turnRadius;
    }

    public static float Scale(float oldMin, float oldMax, float newMin, float newMax, float oldValue)
    {
        float oldRange = (oldMax - oldMin);
        float newRange = (newMax - newMin);
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        Debug.Log(newValue);
        return (newValue);
    }

    float _naiveRotationForce = 5;
    float _naiveRotationDampeningForce = -10;
    
    private void NaiveGroundControl()
    {
        if (!_state.IsDrive) return;

        // Throttle
        var throttleInput = Input.GetAxis("Vertical");
        float Fx = throttleInput * GetForwardAcceleration(forwardSpeedAbs);
        _rigidbody.AddForceAtPosition(Fx * transform.forward, _rigidbody.transform.TransformPoint(_rigidbody.centerOfMass),
            ForceMode.Acceleration);

        // Auto dampening
        _rigidbody.AddForce(transform.forward * (5.25f * -Mathf.Sign(forwardSpeed) * (1 - Mathf.Abs(throttleInput))),
            ForceMode.Acceleration);
        // alternative auto dampening
        //if (throttleInput == 0) _rb.AddForce(transform.forward * (5.25f * -Mathf.Sign(forwardSpeed)), ForceMode.Acceleration); 

        // Steering
        _rigidbody.AddTorque(transform.up * (Input.GetAxis("Horizontal") * _naiveRotationForce), ForceMode.Acceleration);
        _rigidbody.AddTorque(transform.up * (_naiveRotationDampeningForce * (1 - Mathf.Abs(Input.GetAxis("Horizontal"))) *
                                      transform.InverseTransformDirection(_rigidbody.angularVelocity).y), ForceMode.Acceleration);
    }
}
