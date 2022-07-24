using Photon.Pun;
using TMPro;
using UnityEngine;

public class GameRoomTimeDisplay : MonoBehaviour
{
    [SerializeField] GameObject GameManager;
    private OnlineGameManager _manager;

    private TextMeshProUGUI timeLabel;

    private bool isEnd = false;

    private void Start()
    {
        _manager = GameManager.GetComponent<OnlineGameManager>();
        timeLabel = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (_manager.DisplayMinutes == 0 && _manager.DisplaySeconds == 0)
        {
            isEnd = true;
        }
        if (isEnd)
        {
            timeLabel.text = "0:00";
        }
        else
        {
            timeLabel.text = "" + _manager.DisplayMinutes.ToString("0") + ":" + _manager.DisplaySeconds.ToString("00");
        }
    }
}