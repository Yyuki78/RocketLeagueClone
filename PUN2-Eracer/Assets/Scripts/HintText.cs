using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintText : MonoBehaviour
{
    [SerializeField] GameObject hintText;

    [SerializeField] GameObject RocketCar;
    private CarState _state;

    private bool once = false;

    // Start is called before the first frame update
    void Start()
    {
        _state = RocketCar.GetComponent<CarState>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_state._states == CarState.CarStates.BodyGroundDead && !once)
        {
            once = true;
            StartCoroutine(DisplayText());
        }

        if (_state._states != CarState.CarStates.BodyGroundDead)
        {
            hintText.SetActive(false);
        }
    }

    private IEnumerator DisplayText()
    {
        yield return new WaitForSeconds(2f);
        once = false;
        if (_state._states != CarState.CarStates.BodyGroundDead) yield break;
        hintText.SetActive(true);
        yield break;
    }
}
