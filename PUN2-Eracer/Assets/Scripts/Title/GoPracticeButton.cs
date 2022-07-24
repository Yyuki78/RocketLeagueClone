using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GoPracticeButton : MonoBehaviourPunCallbacks
{
    public void OnButtonClick()
    {
        //PhotonNetwork.Disconnect();
        SceneManager.LoadScene("SampleScene");
    }
    /*
    public void OnButtonClick2()
    {
        PhotonNetwork.Disconnect();
    }

    public void OnButtonClick3()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnButtonClick4()
    {
        PhotonNetwork.Reconnect();
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("SampleScene");
    }*/
}
