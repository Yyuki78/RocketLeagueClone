using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController1 : MonoBehaviour
{
    [SerializeField] GameObject CarCameraObj;
    private Transform _carCameraTransform;
    [SerializeField] CinemachineVirtualCamera _carCamera;
    [SerializeField] CinemachineVirtualCamera _ballCamera;

    private Vector3 _lastCarTransform;
    [SerializeField] GameObject RocketCar;
    private CarState _state;
    private Transform _carLocalTransform;
    private Vector3 _DifferenceTransform;

    [SerializeField] int Mode;//0なら練習,1なら試合
    [SerializeField] GameObject CameraText;

    CinemachineTargetGroup cinemachineTargetGroup;

    private bool waitRag = true;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("Set", 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (waitRag) return;
        _DifferenceTransform = _carLocalTransform.position - _lastCarTransform;
        if (GameManager.InputManager.isSwitchCamera)
        {
            _carCamera.Priority = 5;
            _ballCamera.Priority = 10;
            if (Mode == 0)
                CameraText.SetActive(true);
        }
        else
        {
            _carCamera.Priority = 10;
            _ballCamera.Priority = 5;
            if (Mode == 0)
                CameraText.SetActive(false);
        }
        /*
        _carCameraTransform.transform.position = _carLocalTransform.position + new Vector3(0, 1, -3f);
        if (_state.IsDrive)
        {
            _carCameraTransform.transform.rotation = _carLocalTransform.rotation;
        }
        if (GameManager.InputManager.throttleInput < 0)
        {
            if (GameManager.InputManager.isBoost) return;
            //_carCameraTransform.transform.position -= _carLocalTransform.position;
        }

        _lastCarTransform = _carLocalTransform.position;
        */
    }

    private void Set()
    {
        if (Mode == 0)
        {
            _carCameraTransform = CarCameraObj.GetComponent<Transform>();
            _carLocalTransform = RocketCar.GetComponent<Transform>();
            _state = RocketCar.GetComponent<CarState>();
        }
        else
        {
            RocketCar = GameObject.FindWithTag("Player");
            _carCameraTransform = CarCameraObj.GetComponent<Transform>();
            _carLocalTransform = RocketCar.GetComponent<Transform>();
            _state = RocketCar.GetComponent<CarState>();
            cinemachineTargetGroup = GetComponentInChildren<CinemachineTargetGroup>();
            cinemachineTargetGroup.AddMember(RocketCar.transform, 1, 1);
        }
        waitRag = false;
    }
}
