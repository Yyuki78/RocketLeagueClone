using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallEffect : MonoBehaviour
{
    private Rigidbody _rigidbody;
    [SerializeField] GameObject BallTrail;
    private TrailRenderer _trail;
    [SerializeField] GameObject GroundBallPos;
    private Vector3 _groundPos;
    [SerializeField] Gradient _gradient;
    [SerializeField] Gradient _offGradient;

    [SerializeField] AudioSource _audio1;
    [SerializeField] AudioSource _audio2;
    [SerializeField] AudioClip _clip1;
    [SerializeField] AudioClip _clip2;
    private bool once = true;
    private bool once2 = true;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _trail = GetComponentInChildren<TrailRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _groundPos = transform.position;
        _groundPos.y = 9.45f;
        GroundBallPos.transform.position = _groundPos;

        if (_rigidbody.velocity.magnitude >= 15f)
        {
            BallTrail.SetActive(true);
            _trail.colorGradient = _gradient;
            _trail.time = 1f + (_rigidbody.velocity.magnitude - 20f) / 10f;
            _trail.startWidth = 0.6f;
        }
        else
        {
            BallTrail.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 10)
        {
            _audio1.volume = _rigidbody.velocity.magnitude / (23f * 6f);
            if (once)
            {
                _audio1.PlayOneShot(_clip1);
                once = false;
                StartCoroutine(ResetSE());
            }
        }

        if (collision.gameObject.layer == 8)
        {
            var _rigid = collision.gameObject.GetComponent<Rigidbody>();
            _audio2.volume = 0.05f + (_rigidbody.velocity.magnitude + _rigid.velocity.magnitude) / (23f * 10f);
            if (once2)
            {
                _audio2.PlayOneShot(_clip2);
                once2 = false;
                StartCoroutine(ResetSE2());
            }
        }
    }

    private IEnumerator ResetSE()
    {
        yield return new WaitForSeconds(0.1f);
        once = true;
    }

    private IEnumerator ResetSE2()
    {
        yield return new WaitForSeconds(0.1f);
        once2 = true;
    }

    private void OnEnable()
    {
        once = true;
        once2 = true;
    }
}
