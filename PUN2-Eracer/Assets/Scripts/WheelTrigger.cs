﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTrigger : MonoBehaviour
{
    //コライダーのみだと簡単に離れた判定になってしまうので
    //Rayを使った判定を追加する

    private CarState _state;
    private CarMove2 _move;

    public bool Hitting = false;

    private bool HittingCol = false;

    private float _rayLen, _rayOffset = 0.05f;
    private Vector3 _rayContactPoint, _rayContactNormal;

    Rigidbody _rigidbody;

    private void Start()
    {
        _state = GetComponentInParent<CarState>();
        _move = GetComponentInParent<CarMove2>();
        _rigidbody = GetComponentInParent<Rigidbody>();
        _rayLen = transform.localScale.x / 2 + _rayOffset;
    }

    private void FixedUpdate()
    {
        Hitting = IsRayContact() || HittingCol;

        if (Hitting/* && _state.IsDrive*/)
            ApplyStickyForces(StickyForceConstant * 5, _rayContactPoint, -_rayContactNormal);

        if (!Hitting && _state.SomeWheelHit)
            ApplyStickyForces2(StickyForceConstant * 5, _rayContactPoint, -_rayContactNormal);
    }

    //タイヤを地面から離れなくする
    const int StickyForceConstant = 325 / 100;
    private void ApplyStickyForces(float stickyForce, Vector3 position, Vector3 dir)
    {
        //Vector3 force = stickyForce * Mathf.Abs(_move.currentSteerAngle) / 4 / 4 * dir * Mathf.Abs(_move.forwardSpeed) / 20;//Mathf.Abs(_move.currentSteerAngle)/4倍 +  Mathf.Abs(_move.forwardSpeed) / 20倍
        Vector3 force = stickyForce * 4 / 4 * dir;
        //if (GameManager.InputManager.isDrift) force = force * 2;

        _rigidbody.AddForceAtPosition(force, position, ForceMode.Acceleration);
    }

    private void ApplyStickyForces2(float stickyForce, Vector3 position, Vector3 dir)
    {
        Vector3 force = stickyForce * 12*2 / 4 * dir;//Mathf.Abs(_move.currentSteerAngle)/8倍

        _rigidbody.AddForceAtPosition(force, position, ForceMode.Acceleration);
    }

    private bool IsRayContact()
    {
        var isHit = Physics.Raycast(transform.position, -transform.up, out var hit, _rayLen);
        _rayContactPoint = hit.point;
        _rayContactNormal = hit.normal;
        return false || isHit;
    }

    private void OnTriggerEnter(Collider other)
    {
        HittingCol = true;
    }

    private void OnTriggerExit(Collider other)
    {
        HittingCol = false;
    }
}
