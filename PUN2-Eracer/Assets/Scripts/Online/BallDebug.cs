using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BallDebug : MonoBehaviourPunCallbacks, IOnPhotonViewOwnerChange
{
    public void ChangeOwner(Player NewOwner)
    {
        // 所有権の移譲
        //gameObject.GetComponent<PhotonView>().TransferOwnership(NewOwner);
        gameObject.GetComponent<PhotonView>().RequestOwnership();
    }

    void IOnPhotonViewOwnerChange.OnOwnerChange(Player newOwner, Player previousOwner)//所有者が変わったことを知らせる関数
    {
        string objectName = $"{photonView.name}({photonView.ViewID})";
        string oldName = previousOwner.NickName;
        string newName = newOwner.NickName;
        Debug.Log($"{objectName} の所有者が {oldName} から {newName} に変更されました");
    }
}
