using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CarParticleSystem : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject BoostParticle;
    [SerializeField] GameObject MaxSpeedTrails;
    [SerializeField] GameObject FlipParticles;
    [SerializeField] GameObject WindParticle;
    [SerializeField] GameObject JumpParticle;
    [SerializeField] GameObject SteerParticles;

    private CarMove3 _move;
    private CarState _state;
    private Rigidbody _rigidbody;
    private CarJumpMove _jump;

    // Start is called before the first frame update
    void Start()
    {
        _move = GetComponentInParent<CarMove3>();
        _state = GetComponentInParent<CarState>();
        _rigidbody = GetComponentInParent<Rigidbody>();
        _jump = GetComponentInParent<CarJumpMove>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_move.isMoving) return;
        if (_move.IsOnline)
        {
            if (photonView.IsMine)
            {
                if (GameManager.InputManager.isBoost && _move.BoostQuantity != 0)
                {
                    BoostParticle.SetActive(true);
                }
                else
                {
                    BoostParticle.SetActive(false);
                }

                if (_jump.Jumping)
                {
                    JumpParticle.SetActive(true);
                    StartCoroutine(StopParticle1());
                }
                else
                {
                    JumpParticle.SetActive(false);
                }

                if (_jump.SecondJumping)
                {
                    FlipParticles.SetActive(true);
                    StartCoroutine(StopParticle2());
                }
                else
                {
                    FlipParticles.SetActive(false);
                }

                if (_rigidbody.velocity.magnitude >= _move.m_Topspeed2 - 0.5f)
                {
                    WindParticle.SetActive(true);
                }
                else
                {
                    WindParticle.SetActive(false);
                }

                if (Mathf.Abs(GameManager.InputManager.steerInput) >= 0.2f && _state.IsDrive && Mathf.Abs(_rigidbody.velocity.magnitude) > 1f)
                {
                    SteerParticles.SetActive(true);
                }
                else
                {
                    SteerParticles.SetActive(false);
                }
            }
        }
        else
        {
            if (GameManager.InputManager.isBoost && _move.BoostQuantity != 0)
            {
                BoostParticle.SetActive(true);
            }
            else
            {
                BoostParticle.SetActive(false);
            }

            if (_jump.Jumping)
            {
                JumpParticle.SetActive(true);
                StartCoroutine(StopParticle1());
            }
            else
            {
                JumpParticle.SetActive(false);
            }

            if (_jump.SecondJumping)
            {
                FlipParticles.SetActive(true);
                StartCoroutine(StopParticle2());
            }
            else
            {
                FlipParticles.SetActive(false);
            }

            if (_rigidbody.velocity.magnitude >= _move.m_Topspeed2 - 0.5f)
            {
                WindParticle.SetActive(true);
            }
            else
            {
                WindParticle.SetActive(false);
            }

            if (Mathf.Abs(GameManager.InputManager.steerInput) >= 0.2f && _state.IsDrive && Mathf.Abs(_rigidbody.velocity.magnitude) > 1f)
            {
                SteerParticles.SetActive(true);
            }
            else
            {
                SteerParticles.SetActive(false);
            }
        }
        
    }

    void FixedUpdate()
    {
        if (_move.holdSpeed && _state.IsDrive)
        {
            MaxSpeedTrails.SetActive(true);
        }
        else
        {
            MaxSpeedTrails.SetActive(false);
        }
    }

    private IEnumerator StopParticle1()
    {
        yield return new WaitForSeconds(0.5f);
        _jump.Jumping = false;
    }

    private IEnumerator StopParticle2()
    {
        yield return new WaitForSeconds(0.75f);
        _jump.SecondJumping = false;
    }
}
