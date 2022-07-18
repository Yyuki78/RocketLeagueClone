using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreText : MonoBehaviour
{
    [SerializeField] GameObject GameManager;
    private OnlineGameManager _manager;

    private TextMeshProUGUI timeLabel;

    private bool startCol = false;

    private int ScoreBlue = 0;
    private int ScoreRed = 0;

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameManager.GetComponent<OnlineGameManager>();
        timeLabel = GetComponent<TextMeshProUGUI>();
        timeLabel.text = "0 対 0";
    }

    // Update is called once per frame
    void Update()
    {
        if (_manager.isGoalBlue || _manager.isGoalRed)
        {
            if (!startCol)
            {
                startCol = true;
                StartCoroutine(Countdown());
            }
        }
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.1f);
        if (!startCol) yield break;
        if (_manager.isGoalBlue)
            ScoreRed++;
        if (_manager.isGoalRed)
            ScoreBlue++;
        timeLabel.text = ScoreBlue.ToString() + " 対 " + ScoreRed.ToString();
        yield return new WaitForSeconds(7.0f);
        startCol = false;
        yield break;
    }
}
