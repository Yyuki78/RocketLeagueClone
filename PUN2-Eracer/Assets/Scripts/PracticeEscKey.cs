using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PracticeEscKey : MonoBehaviour
{
    [SerializeField] GameObject Menu;

    private bool isStop = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isStop = !isStop;
        }
        if (isStop)
        {
            Time.timeScale = 0f;
            Menu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            Menu.SetActive(false);
        }
    }

    public void ClickResumeButton()
    {
        isStop = !isStop;
    }

    public void ClickTitleButton()
    {
        isStop = !isStop;
        SceneManager.LoadScene("TitleScene");
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("TitleScene");
    }
}
