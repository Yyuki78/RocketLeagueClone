using Photon.Pun;
using TMPro;
using UnityEngine;

public class GameRoomTimeDisplay : MonoBehaviour
{
    [SerializeField] GameObject GameManager;
    private OnlineGameManager _manager;

    private TextMeshProUGUI timeLabel;

    private void Start()
    {
        _manager = GameManager.GetComponent<OnlineGameManager>();
        timeLabel = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        timeLabel.text = "残り時間:" + _manager.DisplayMinutes.ToString("0") + ":" + _manager.DisplaySeconds.ToString("00");
    }
}