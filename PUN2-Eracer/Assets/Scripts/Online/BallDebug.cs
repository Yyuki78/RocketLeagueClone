using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BallDebug : MonoBehaviourPun, IOnPhotonViewOwnerChange, IOnPhotonViewControllerChange
{
    private void Start()
    {
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
    }

    public bool once = true;
    public void ChangeOwner(Player NewOwner)
    {
        if (!once) return;
        once = false;
        // 所有権の移譲
        gameObject.GetComponent<PhotonView>().TransferOwnership(NewOwner);
        //gameObject.GetComponent<PhotonView>().RequestOwnership();
    }

    private void OnEnable()
    {
        // PhotonViewのコールバック対象に登録する
        photonView.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        // PhotonViewのコールバック対象の登録を解除する
        photonView.RemoveCallbackTarget(this);
    }

    // ネットワークオブジェクトの所有者が変更された時に呼ばれるコールバック
    void IOnPhotonViewOwnerChange.OnOwnerChange(Player newOwner, Player previousOwner)
    {
        string objectName = $"{photonView.name}({photonView.ViewID})";
        string oldName = previousOwner.NickName;
        string newName = newOwner.NickName;
        Debug.Log($"{objectName} の所有者が {oldName} から {newName} に変更されました");
    }

    // ネットワークオブジェクトの管理者が変更された時に呼ばれるコールバック
    void IOnPhotonViewControllerChange.OnControllerChange(Player newController, Player previousController)
    {
        string objectName = $"{photonView.name}({photonView.ViewID})";
        string oldName = previousController.NickName;
        string newName = newController.NickName;
        //Debug.Log($"{objectName} の管理者が {oldName} から {newName} に変更されました");
    }
}
