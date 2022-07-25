using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GoTitleButton : MonoBehaviourPunCallbacks
{
    public void ClickTitleButton()
    {
        //PhotonNetwork.AutomaticallySyncScene = false;
        // ルームから退出する
        PhotonNetwork.LeaveRoom();
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.1f);
        PhotonNetwork.Disconnect();
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("TitleScene");
    }
}
