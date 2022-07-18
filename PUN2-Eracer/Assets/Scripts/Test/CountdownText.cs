using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountdownText : MonoBehaviour
{
    [SerializeField] GameObject GameManager;
    private OnlineGameManager _manager;

    private TextMeshProUGUI timeLabel;
    // Start is called before the first frame update
    void Start()
    {
        _manager = GameManager.GetComponent<OnlineGameManager>();
        timeLabel = GetComponent<TextMeshProUGUI>();
        timeLabel.text = " ";
    }

    // Update is called once per frame
    void Update()
    {
        if (_manager.isCountdown)
        {
            _manager.isCountdown = false;
            StartCoroutine(Countdown());
        }
    }

    private IEnumerator Countdown()
    {
        timeLabel.text = "3";
        yield return new WaitForSeconds(1.3f);
        timeLabel.text = "2";
        yield return new WaitForSeconds(1.0f);
        timeLabel.text = "1";
        yield return new WaitForSeconds(1.0f);
        timeLabel.text = "スタート!";
        yield return new WaitForSeconds(1.0f);
        timeLabel.text = " ";
        yield break;
    }
}
