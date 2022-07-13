using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RequestBallOwner : MonoBehaviourPunCallbacks,IOnPhotonViewOwnerChange
{
    [SerializeField] PhotonView _myPhotonView;
    private PhotonView _photonView;

    [SerializeField] GameObject BodyM;

    private bool canChange = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            Debug.Log("ボールと接触");
            _photonView = other.gameObject.GetComponent<PhotonView>();
            /*_photonView = collision.gameObject.GetComponent<PhotonView>();
            var player = _photonView.Owner;
            PhotonNetwork.SetMasterClient(player);
            */
            /*
            if (_photonView.IsMine) return;
            _photonView.RequestOwnership();
            */
            if (_photonView.IsMine) return;
            if (!canChange) return;
            if (_photonView.Owner == gameObject.GetComponent<PhotonView>().Owner) return;
            // 所有権の移譲
            ////////other.gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
            other.gameObject.GetComponent<BallDebug>().ChangeOwner(PhotonNetwork.LocalPlayer);
            Debug.Log(PhotonNetwork.LocalPlayer);
            canChange = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        StartCoroutine(coolDown());
    }

    private IEnumerator coolDown()
    {
        yield return new WaitForSeconds(0.5f);
        canChange = true;
        yield break;
    }
    /*
    private void sendOwner(GameObject obj)//人形に所有者を渡すための関数
    {
        DollSync dollSync = obj.GetComponent<DollSync>();//スクリプトを取得する
        if (dollSync == null) return;//人形用のスクリプトを持っていなかったらreturnする
        dollSync.ChangeOwner(PhotonNetwork.LocalPlayer);

    }*/

    public void ChangeOwner(Player NewOwner)
    {
        // 所有権の移譲
        gameObject.GetComponent<PhotonView>().TransferOwnership(NewOwner);
    }

    void IOnPhotonViewOwnerChange.OnOwnerChange(Player newOwner, Player previousOwner)//所有者が変わったことを知らせる関数
    {
        string objectName = $"{photonView.name}({photonView.ViewID})";
        string oldName = previousOwner.NickName;
        string newName = newOwner.NickName;
        Debug.Log($"{objectName} の所有者が {oldName} から {newName} に変更されました");
    }

    //PhotonView.OwnershipTransfer == OwnershipOption.Request の時に呼ばれる
    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {

    }

    //PhotonView.OwnershipTransfer == OwnershipOption.Takeover の時に呼ばれる
    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        
    }
}
