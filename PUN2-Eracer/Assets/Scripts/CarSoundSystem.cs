using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CarSoundSystem : MonoBehaviourPunCallbacks
{
    [SerializeField] AudioSource _audio1;//エンジン音
    [SerializeField] AudioSource _audio2;//ブースト開始音
    [SerializeField] AudioSource _audio3;//ブースト音
    [SerializeField] AudioSource _audio4;//曲がる時の音(ドリフト)
    [SerializeField] AudioSource _audio5;//ジャンプ音
    [SerializeField] AudioSource _audio6;//着地音
    [SerializeField] AudioSource _audio7;//ボールとの衝突音
    [SerializeField] AudioSource _audio8;//体と地面の接触音
    [SerializeField] AudioSource _audio9;//最大加速時の音
    [SerializeField] AudioSource _audio10;//待機音

    [SerializeField] AudioClip _clip1;//待機音
    [SerializeField] AudioClip _clip2;//エンジン音
    [SerializeField] AudioClip _clip3;//ブースト開始音
    [SerializeField] AudioClip _clip4;//ブースト音
    [SerializeField] AudioClip _clip5;//曲がる時の音(ドリフト)
    [SerializeField] AudioClip _clip6;//ジャンプ音
    [SerializeField] AudioClip _clip7;//フリップ音
    [SerializeField] AudioClip _clip8;//着地音
    [SerializeField] AudioClip _clip9;//ボールとの衝突音
    [SerializeField] AudioClip _clip10;//体と地面の接触音
    [SerializeField] AudioClip _clip11;//最大加速時の音

    private CarMove3 _move;
    private CarState _state;
    private Rigidbody _rigidbody;
    private CarJumpMove _jump;

    private bool boostOnce = true;
    private bool jumpOnce = true;
    private bool steerOnce = true;

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
        if (_move.IsOnline)
        {
            if (photonView.IsMine)
            {
                
            }
        }
        else
        {
            if (GameManager.InputManager.isBoost && _move.BoostQuantity != 0)
            {
                if (!_move.isMoving) return;
                if (boostOnce)
                {
                    boostOnce = false;
                    _audio2.PlayOneShot(_clip3);
                    _audio3.PlayOneShot(_clip4);
                }
            }
            else
            {
                _audio3.Stop();
                boostOnce = true;
            }

            if (_jump.Jumping)
            {
                if (jumpOnce)
                {
                    jumpOnce = false;
                    _audio5.PlayOneShot(_clip6);
                }
            }
            else
            {
                jumpOnce = true;
            }

            if (_jump.SecondJumping)
            {

            }
            else
            {

            }

            if (_rigidbody.velocity.magnitude >= 1f)
            {
                _audio1.volume = 0.02f+_rigidbody.velocity.magnitude / (23 * 12.5f);
                _audio1.Play();
                _audio10.Stop();
            }
            else
            {
                _audio1.Stop();
                _audio10.Play();
            }

            if (_rigidbody.velocity.magnitude >= _move.m_Topspeed2 - 0.5f)
            {

            }
            else
            {

            }

            if (Mathf.Abs(GameManager.InputManager.steerInput) >= 0.2f && _state.IsDrive && Mathf.Abs(_rigidbody.velocity.magnitude) > 1f)
            {
                if (GameManager.InputManager.isDrift)
                {
                    _audio4.volume = 0.1f + Mathf.Abs(GameManager.InputManager.steerInput) / 25f;
                }
                else
                {
                    _audio4.volume = 0.01f + Mathf.Abs(GameManager.InputManager.steerInput) / 50f;
                }
                if (steerOnce)
                {
                    steerOnce = false;
                    _audio4.Play();
                }
            }
            else
            {
                _audio4.Stop();
                steerOnce = true;
            }
        }
    }
}
