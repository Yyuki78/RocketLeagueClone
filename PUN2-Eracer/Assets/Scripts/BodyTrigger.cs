using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTrigger : MonoBehaviour
{
    private CarState _state;
    // Start is called before the first frame update
    void Start()
    {
        _state = GetComponentInParent<CarState>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _state.BodyHitting = true;
    }

    private void OnTriggerExit(Collider other)
    {
        _state.BodyHitting = false;
    }
}
