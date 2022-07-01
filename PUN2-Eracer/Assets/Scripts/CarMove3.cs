using System;
using UnityEngine;

internal enum SpeedType
{
    MPH,
    KPH
}

public class CarMove3 : MonoBehaviour
{
    [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
    [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
    [SerializeField] private Vector3 m_CentreOfMassOffset;
    [SerializeField] private float m_MaximumSteerAngle;
    [Range(0, 1)] [SerializeField] private float m_SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing
    [Range(0, 1)] [SerializeField] private float m_TractionControl; // 0 is no traction control, 1 is full interference
    [SerializeField] private float m_FullTorqueOverAllWheels;
    [SerializeField] private float m_ReverseTorque;
    [SerializeField] private float m_MaxHandbrakeTorque;
    [SerializeField] private float m_Downforce = 100f;
    [SerializeField] private SpeedType m_SpeedType;
    //public float m_Topspeed = 200;
    [SerializeField] private static int NoOfGears = 5;
    [SerializeField] private float m_RevRangeBoundary = 1f;
    [SerializeField] private float m_SlipLimit;
    [SerializeField] private float m_BrakeTorque;

    private Quaternion[] m_WheelMeshLocalRotations;
    private Vector3 m_Prevpos, m_Pos;
    private float m_SteerAngle;
    private int m_GearNum;
    private float m_GearFactor;
    private float m_OldRotation;
    private float m_CurrentTorque;
    private Rigidbody m_Rigidbody;
    private const float k_ReversingThreshold = 0.01f;

    public float m_Topspeed2 = 23;
    private bool holdSpeed = false;

    public bool Skidding { get; private set; }
    public float BrakeInput { get; private set; }
    public float CurrentSteerAngle { get { return m_SteerAngle; } }
    public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } }
    //public float MaxSpeed { get { return m_Topspeed; } }
    //public float Revs { get; private set; }
    public float AccelInput { get; private set; }

    CarState _state;

    public float BoostQuantity = 30;

    // Use this for initialization
    private void Start()
    {
        m_WheelMeshLocalRotations = new Quaternion[4];
        for (int i = 0; i < 4; i++)
        {
            m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
        }
        m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

        m_MaxHandbrakeTorque = float.MaxValue;

        m_Rigidbody = GetComponent<Rigidbody>();
        m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);

        _state = GetComponent<CarState>();
    }

    private void FixedUpdate()
    {
        float h = GameManager.InputManager.steerInput;
        float v = GameManager.InputManager.throttleInput;
        float handbrake = 0;
        if (_state.IsDrive)
        {
            Move(h, v, v, handbrake);
        }
    }

    /*
    private void GearChanging()
    {
        float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
        float upgearlimit = (1 / (float)NoOfGears) * (m_GearNum + 1);
        float downgearlimit = (1 / (float)NoOfGears) * m_GearNum;

        if (m_GearNum > 0 && f < downgearlimit)
        {
            m_GearNum--;
        }

        if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
        {
            m_GearNum++;
        }
    }


    // simple function to add a curved bias towards 1 for a value in the 0-1 range
    private static float CurveFactor(float factor)
    {
        return 1 - (1 - factor) * (1 - factor);
    }


    // unclamped version of Lerp, to allow value to exceed the from-to range
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }

    /*
    private void CalculateGearFactor()
    {
        float f = (1 / (float)NoOfGears);
        // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
        // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
        var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
        m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
    }*/

    /*
    private void CalculateRevs()
    {
        // calculate engine revs (for display / sound)
        // (this is done in retrospect - revs are not used in force/power calculations)
        CalculateGearFactor();
        var gearNumFactor = m_GearNum / (float)NoOfGears;
        var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
        var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
        Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
    }*/


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

        //SteerHelper();
        //進ませる
        ApplyDrive(accel, footbrake);
        //ブレーキ
        ApplyBrake(BrakeInput);

        //地上のブースト　AirBoostingはCarAirMove
        GroundBoosting();

        //CapSpeed();

        //最大速度に到達するとその速度で走るようになる
        HoldTopSpeed();

        //スタックする問題の解消
        ResolveStack();


        //CalculateRevs();
        //GearChanging();

        //AddDownForce();
        //TractionControl();
    }

    /*
    private void CapSpeed()
    {
        float speed = m_Rigidbody.velocity.magnitude;
        switch (m_SpeedType)
        {
            case SpeedType.MPH:

                speed *= 2.23693629f;
                if (speed > m_Topspeed)
                    m_Rigidbody.velocity = (m_Topspeed / 2.23693629f) * m_Rigidbody.velocity.normalized;
                break;

            case SpeedType.KPH:
                speed *= 3.6f;
                if (speed > m_Topspeed)
                    m_Rigidbody.velocity = (m_Topspeed / 3.6f) * m_Rigidbody.velocity.normalized;
                break;
        }
    }*/


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
            BoostQuantity -= 0.5f;
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
            m_WheelColliders[2].steerAngle = -m_SteerAngle / 10;
            m_WheelColliders[3].steerAngle = -m_SteerAngle / 10;

            //m_WheelColliders[2].steerAngle = -Mathf.Abs(m_SteerAngle) * driftSteer / 12;
            //m_WheelColliders[3].steerAngle = -Mathf.Abs(m_SteerAngle) * driftSteer / 12;

            if (Mathf.Abs(steering) >= 0.1f)
            {
                if (driftSteerTime < 3f)
                {
                    driftSteerTime += Time.deltaTime;
                }
            }
            else
            {
                driftSteerTime = 0f;
            }

            sFriction.extremumSlip = 1f + (driftSteerTime / 3 * 1);//最大2
            sFriction.extremumValue = 30f - (driftSteerTime / 3 * 20);//最小10

            Spring.spring = 11500 - (driftSteerTime / 3 * 9000);//最小2500
        }
        else
        {
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

    /*
    private void SteerHelper()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            m_WheelColliders[i].GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return; // wheels arent on the ground so dont realign the rigidbody velocity
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
        }
        m_OldRotation = transform.eulerAngles.y;
    }*/

    /*
    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        /*
        m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up * m_Downforce * 5 *
                                                     m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
        *//*
        m_Rigidbody.AddForce(-transform.up * m_Downforce);
        if (_state.SomeWheelHit && !_state.IsDrive)
        {
            m_Rigidbody.AddForce(-transform.up * m_Downforce * 5);
        }
    }
    /*
    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        WheelHit wheelHit;

        for (int i = 0; i < 4; i++)
        {
            m_WheelColliders[i].GetGroundHit(out wheelHit);

            AdjustTorque(wheelHit.forwardSlip);
        }
    }

    
    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
        {
            m_CurrentTorque -= 10 * m_TractionControl;
        }
        else
        {
            m_CurrentTorque += 10 * m_TractionControl;
            if (m_CurrentTorque > m_FullTorqueOverAllWheels)
            {
                m_CurrentTorque = m_FullTorqueOverAllWheels;
            }
        }
    }*/

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
}
