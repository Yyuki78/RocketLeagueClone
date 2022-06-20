using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelForce : MonoBehaviour
{
    float Fy;

    [SerializeField] bool wheelFL, wheelFR, wheelRL, wheelRR;
    private WheelTrigger _wtrigger;

    public Transform wheelMesh;
    private float _meshRevolutionAngle;

    private Rigidbody _rigidbody;
    private CarState _state;
    private CarMove2 _move;

    private float _wheelRadius, _wheelForwardVelocity, _wheelLateralVelocity;
    private Vector3 _wheelVelocity, _lastWheelVelocity, _wheelAcceleration, _wheelContactPoint, _lateralForcePosition = Vector3.zero;

    const float AutoBrakeAcceleration = 5.25f;

    //[HideInInspector]
    public bool isDrawWheelVelocities, isDrawWheelDisc, isDrawForces;

    //威力変更用
    private float DebugValue = 4f;

    void Start()
    {
        _wtrigger = GetComponent<WheelTrigger>();
        _rigidbody = GetComponentInParent<Rigidbody>();
        _state = GetComponentInParent<CarState>();
        _move = GetComponentInParent<CarMove2>();
        _wheelRadius = transform.localScale.z / 2;
    }

    //タイヤを回す　CarMove2で使用
    public void RotateWheels(float steerAngle)
    {
        if (wheelFL || wheelFR)
        {
            transform.localRotation = Quaternion.Euler(Vector3.up * steerAngle);
        }
        
        // Update mesh rotations of the wheel
        if (wheelMesh)
        {
            //wheelMesh.transform.position = transform.position;
            wheelMesh.transform.localRotation = transform.localRotation;
            _meshRevolutionAngle += (Time.deltaTime * transform.InverseTransformDirection(_wheelVelocity).z) /
                (2 * Mathf.PI * _wheelRadius) * 360;
            wheelMesh.transform.Rotate(Vector3.right, _meshRevolutionAngle * 1.3f);
            //transform.Rotate(new Vector3(0, 1, 0), steerAngle - transform.localEulerAngles.y);
        }
    }

    private void FixedUpdate()
    {
        UpdateWheelState();

        //DownForce();

        if (!_state.IsDrive) return;
        //ApplyForwardForce();
        ApplyLateralForce();
        SimulateDrag();
    }

    /*
    //車の下方向への重力
    private void DownForce()
    {
        if (wheelMesh.transform.position.y > 42f) return;

        if (_state.IsDrive)
        {
            _rigidbody.AddForce(-transform.up * 5, ForceMode.Acceleration);
        }

        if (_state.IsDrive && !_wtrigger.Hitting)
        {
            _rigidbody.AddForce(-transform.up * 5, ForceMode.Acceleration);
        }
    }*/
    
    //前後に移動する　CarMove2で使用
    public void ApplyForwardForce(float force)
    {
        _rigidbody.AddForce(force * transform.forward, ForceMode.Acceleration);

        // Kill velocity to 0 for small car velocities
        if (force == 0 && _move.forwardSpeedAbs < 0.1)
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, 0);
    }

    //横力の再現
    private void ApplyLateralForce()
    {
        Fy = _wheelLateralVelocity * _move.currentWheelSideFriction;
        _lateralForcePosition = transform.localPosition;
        _lateralForcePosition.y = wheelMesh.localPosition.y-0.1f;

        _lateralForcePosition = _move.transform.TransformPoint(_lateralForcePosition);
        _rigidbody.AddForceAtPosition(-Fy * transform.right, _lateralForcePosition, ForceMode.Acceleration);
    }

    //摩擦の再現　減速する
    private void SimulateDrag()
    {
        //Applies auto braking if no input, simulates air and ground drag
        if (!(_move.forwardSpeedAbs >= 0.1)) return;
        
        //1/8
        var dragForce = AutoBrakeAcceleration / 32 * _move.forwardSpeedSign * (1 - Mathf.Abs(GameManager.InputManager.throttleInput));
        _rigidbody.AddForce(-dragForce * transform.forward, ForceMode.Acceleration);
    }
    
    //タイヤごとの変数変化纏め
    private void UpdateWheelState()
    {
        _wheelContactPoint = transform.position - transform.up * _wheelRadius;
        _wheelVelocity = _rigidbody.GetPointVelocity(_wheelContactPoint);
        _wheelForwardVelocity = Vector3.Dot(_wheelVelocity, transform.forward);
        _wheelLateralVelocity = Vector3.Dot(_wheelVelocity, transform.right);

        _wheelAcceleration = (_wheelVelocity - _lastWheelVelocity) * Time.fixedTime;
        _lastWheelVelocity = _wheelVelocity;
    }
}
