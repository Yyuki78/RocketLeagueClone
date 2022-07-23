using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class CarMove3 : MonoBehaviourPunCallbacks
{
    private bool isOnline;
    [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
    [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
    [SerializeField] private Vector3 m_CentreOfMassOffset;
    [SerializeField] private float m_MaximumSteerAngle;
    [Range(0, 1)] [SerializeField] private float m_TractionControl; // 0 is no traction control, 1 is full interference
    [SerializeField] private float m_FullTorqueOverAllWheels;
    [SerializeField] private float m_ReverseTorque;
    [SerializeField] private float m_BrakeTorque;

    private Quaternion[] m_WheelMeshLocalRotations;
    private float m_SteerAngle;
    private float m_CurrentTorque;
    private Rigidbody m_Rigidbody;

    public float m_Topspeed2 = 23;
    public bool holdSpeed = false;//CarParticleSystemでも使用
    
    public bool IsOnline { get { return isOnline; } }
    public float BrakeInput { get; private set; }
    public float CurrentSteerAngle { get { return m_SteerAngle; } }
    public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } }
    public float AccelInput { get; private set; }

    CarState _state;

    public float BoostQuantity = 30;

    public bool isMoving = true;

    // Use this for initialization
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "SampleScene")
        { // 練習シーンでのみやりたい処理
            isOnline = false;
        }
        else
        { // それ以外のシーンでやりたい処理
            isOnline = true;
            isMoving = false;
        }
        m_WheelMeshLocalRotations = new Quaternion[4];
        for (int i = 0; i < 4; i++)
        {
            m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
        }
        m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

        m_Rigidbody = GetComponent<Rigidbody>();
        m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);

        _state = GetComponent<CarState>();
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;
        float h = GameManager.InputManager.steerInput;
        float v = GameManager.InputManager.throttleInput;
        float handbrake = 0;
        if (!isOnline)
        {
            if (_state.IsDrive)
            {
                Move(h, v, v, handbrake);
            }
        }
        else
        {
            if (_state.IsDrive && photonView.IsMine)
            {
                Move(h, v, v, handbrake);
            }
        }
    }

    public void Move(float steering, float accel, float footbrake, float handbrake)
    {
        //タイヤのメッシュを動きに追従させる
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            m_WheelColliders[i].GetWorldPose(out position, out quat);
            m_WheelMeshes[i].transform.position = position;
            m_WheelMeshes[i].transform.rotation = quat;
            m_WheelMeshes[i].transform.rotation = m_WheelMeshes[i].transform.rotation *new Quaternion(90, 90, 0, 0);
        }

        //入力量を-1,1の範囲に収める
        steering = Mathf.Clamp(steering, -1, 1);
        AccelInput = accel = Mathf.Clamp(accel, 0, 1);
        BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        //曲げる
        Steering(steering);

        //進ませる
        ApplyDrive(accel, footbrake);
        //ブレーキ
        ApplyBrake(BrakeInput);

        //地上のブースト　AirBoostingはCarAirMove
        GroundBoosting();

        //最大速度に到達するとその速度で走るようになる
        HoldTopSpeed();

        //スタックする問題の解消
        ResolveStack();
    }

    private void ApplyDrive(float accel, float footbrake)
    {
        if (GameManager.InputManager.isBoost && BoostQuantity > 0) return;

        float thrustTorque;

        //前に進ませる
        thrustTorque = accel * (m_CurrentTorque / 4f);
        for (int i = 0; i < 4; i++)
        {
            m_WheelColliders[i].motorTorque = thrustTorque;
        }

        for (int i = 0; i < 4; i++)
        {
            if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, m_Rigidbody.velocity) < 50f)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
            }
            else if (footbrake > 0)
            {
                m_WheelColliders[i].brakeTorque = 0f;
                m_WheelColliders[i].motorTorque = -m_ReverseTorque * footbrake;
            }
        }
    }

    private void ApplyBrake(float footbrake)
    {
        //Set the handbrake.
        //Assuming that wheels 2 and 3 are the rear wheels.
        if (footbrake > 0f)
        {
            var fbTorque = -footbrake * m_BrakeTorque / 4;
            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].motorTorque = fbTorque;
            }
        }
    }

    private void GroundBoosting()
    {
        if (GameManager.InputManager.isBoost)
        {
            if (BoostQuantity <= 0)
            {
                BoostQuantity = 0;
                return;
            }
            BoostQuantity -= 0.4f;
        }

        if (GameManager.InputManager.isBoost && m_Rigidbody.velocity.magnitude < m_Topspeed2 && BoostQuantity > 0)
        {
            if (_state.IsDrive)
            {
                if (holdSpeed) return;
                //m_Rigidbody.AddForce(20 * transform.forward, ForceMode.Acceleration);
                for (int i = 0; i < 4; i++)
                {
                    m_WheelColliders[i].motorTorque = m_FullTorqueOverAllWheels / 1.75f;
                }
            }
        }
    }

    private void HoldTopSpeed()
    {
        if (m_Rigidbody.velocity.magnitude + 0.5f >= m_Topspeed2)
        {
            if (!_state.IsDrive) return;
            holdSpeed = true;
            Debug.Log("最高速度");
        }
        else
        {
            holdSpeed = false;
        }

        if (holdSpeed)
        {
            if (m_Rigidbody.velocity.magnitude >= m_Topspeed2) return;
            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].motorTorque = m_FullTorqueOverAllWheels / 2f;
            }
        }
    }

    float driftSteerTime;
    private void Steering(float steering)
    {
        WheelFrictionCurve sFriction = m_WheelColliders[2].sidewaysFriction;
        JointSpring Spring = m_WheelColliders[2].suspensionSpring;
        //driftSteer += steering;
        //driftSteer= Mathf.Clamp(driftSteer, -10, 10);
        //入力量に応じて左右に曲がる
        m_SteerAngle = m_MaximumSteerAngle;
        /*
        if (steering >= 0.1f)
        {
            //m_SteerAngle -= ((m_Rigidbody.velocity.magnitude) / 3);
            m_SteerAngle -= ((m_Rigidbody.velocity.magnitude) / 3f);
            if (m_SteerAngle < 0)
            {
                m_SteerAngle = 0;
            }
        }
        else if (steering <= -0.1f)
        {
            //m_SteerAngle += ((m_Rigidbody.velocity.magnitude) / 3);
            m_SteerAngle -= ((m_Rigidbody.velocity.magnitude) / 3f);
            if (m_SteerAngle > 0)
            {
                m_SteerAngle = 0;
            }
        }*/
        m_SteerAngle -= ((m_Rigidbody.velocity.magnitude) / 3f);
        m_SteerAngle *= steering;

        //ドリフト中
        if (GameManager.InputManager.isDrift)
        {
            m_WheelColliders[0].steerAngle = m_SteerAngle;
            m_WheelColliders[1].steerAngle = m_SteerAngle;

            //m_WheelColliders[0].steerAngle = Mathf.Abs(m_SteerAngle) * driftSteer / 10;
            //m_WheelColliders[1].steerAngle = Mathf.Abs(m_SteerAngle) * driftSteer / 10;
            /*
            if (steering > 0.1f)
            {
                m_SteerAngle += ((m_Rigidbody.velocity.magnitude) /4);
            }
            else if (steering < -0.1f)
            {
                m_SteerAngle -= ((m_Rigidbody.velocity.magnitude) /4);
            }*/
            m_WheelColliders[2].steerAngle = -m_SteerAngle / 5;
            m_WheelColliders[3].steerAngle = -m_SteerAngle / 5;

            //m_WheelColliders[2].steerAngle = -Mathf.Abs(m_SteerAngle) * driftSteer / 12;
            //m_WheelColliders[3].steerAngle = -Mathf.Abs(m_SteerAngle) * driftSteer / 12;

            if (Mathf.Abs(steering) >= 0.1f)
            {
                if (driftSteerTime < 3f)
                {
                    driftSteerTime += Time.deltaTime;
                }
                else
                {
                    driftSteerTime = 3f;
                }
            }
            else
            {
                driftSteerTime = 0f;
            }

            sFriction.extremumSlip = 1.5f + (driftSteerTime / 3 * 0.5f);//最大2
            sFriction.extremumValue = 20f - (driftSteerTime / 3 * 10);//最小10

            Spring.spring = 9500 - (driftSteerTime / 3 * 7000);//最小2500
        }
        else
        {
            driftSteerTime = 0f;

            m_WheelColliders[0].steerAngle = m_SteerAngle;
            m_WheelColliders[1].steerAngle = m_SteerAngle;
            m_WheelColliders[2].steerAngle = 0;
            m_WheelColliders[3].steerAngle = 0;

            sFriction.extremumSlip = 1f;
            sFriction.extremumValue = 30f;

            Spring.spring = 11500;
        }
        m_WheelColliders[2].sidewaysFriction = sFriction;
        m_WheelColliders[3].sidewaysFriction = sFriction;
        m_WheelColliders[2].suspensionSpring = Spring;
        m_WheelColliders[3].suspensionSpring = Spring;
    }

    private void ResolveStack()
    {
        if (!_state.IsDrive) return;
        if (Mathf.Abs(m_Rigidbody.velocity.magnitude) <= 1)
        {
            m_Rigidbody.AddForce(transform.up * 1f);
        }
    }
    
    public void GetBoostMini()
    {
        BoostQuantity += 12;
        if (BoostQuantity > 100)
        {
            BoostQuantity = 100;
        }
    }

    public void GetBoostMax()
    {
        BoostQuantity = 100;
    }

    public void Respown()
    {
        Move(0, 0, 0, 0);
        for (int i = 0; i < 4; i++)
        {
            m_WheelColliders[i].motorTorque = 0;
        }
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        BoostQuantity = 33f;
    }
}
