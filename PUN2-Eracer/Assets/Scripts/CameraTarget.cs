using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] GameObject RocketCar;
    private CarState _state;
    private Transform _carLocalTransform;
    // Start is called before the first frame update
    void Start()
    {
        RocketCar = GameObject.FindWithTag("Player");
        _carLocalTransform = RocketCar.GetComponent<Transform>();
        _state = RocketCar.GetComponent<CarState>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = _carLocalTransform.position + new Vector3(0, 1f, 0);
        if (_state.IsDrive)
        {
            transform.rotation = _carLocalTransform.rotation;
        }
    }
}
