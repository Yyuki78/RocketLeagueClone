using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoostDrawing : MonoBehaviour
{
    private TextMeshProUGUI _text;
    [SerializeField] Image _image;

    [SerializeField] GameObject RocketCar;
    private CarMove3 _move;

    // Start is called before the first frame update
    void Start()
    {
        RocketCar = GameObject.FindWithTag("Player");
        _text = GetComponent<TextMeshProUGUI>();
        _move = RocketCar.GetComponent<CarMove3>();
        _text.text = _move.BoostQuantity.ToString();
        _image.fillAmount = _move.BoostQuantity * 2f / 300f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _text.text = _move.BoostQuantity.ToString("0");
        _image.fillAmount = _move.BoostQuantity * 2f / 300f;
    }
}
