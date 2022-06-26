using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTrigger : MonoBehaviour
{
    private CarState _state;

    [SerializeField] float _rayLen, _rayOffset = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        _state = GetComponentInParent<CarState>();
        _rayLen = transform.localScale.x / 2 + _rayOffset;
        _rayLen = 0.3f;
    }

    private void Update()
    {
        _state.BodyHitting = IsRayContact();
    }

    private bool IsRayContact()
    {
        var isHit = Physics.Raycast(transform.position, transform.up, out var hit, _rayLen);
        Debug.DrawRay(gameObject.transform.position, transform.up, Color.blue, _rayLen);
        return false || isHit;
    }

    private void OnTriggerEnter(Collider other)
    {
        //_state.BodyHitting = true;
    }

    private void OnTriggerExit(Collider other)
    {
        //_state.BodyHitting = false;
    }
}
