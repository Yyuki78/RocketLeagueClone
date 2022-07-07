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
}
