using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RequestBallOwner : MonoBehaviourPunCallbacks
{
    private PhotonView _myPhotonView;
    private PhotonView _photonView;
    private BallDebug _ballDebug;

    private bool canChange = true;

    private GameObject Ball;

    private float distance = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _myPhotonView = GetComponentInParent<PhotonView>();
        Ball = GameObject.FindWithTag("Ball");
        _photonView = Ball.GetComponent<PhotonView>();
        _ballDebug = Ball.GetComponent<BallDebug>();
    }

    private void FixedUpdate()
    {
        distance = Mathf.Sqrt(Mathf.Pow(gameObject.transform.position.x - Ball.transform.position.x, 2) + Mathf.Pow(gameObject.transform.position.y - Ball.transform.position.y, 2) + Mathf.Pow(gameObject.transform.position.z - Ball.transform.position.z, 2));
        if (distance > 5f)
        {
            canChange = true;
            _ballDebug.once = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            Debug.Log("ボールと接触");
            //_photonView = other.gameObject.GetComponentInParent<PhotonView>();
            
            /*_photonView = collision.gameObject.GetComponent<PhotonView>();
            var player = _photonView.Owner;
            PhotonNetwork.SetMasterClient(player);
            */
            /*
            if (_photonView.IsMine) return;
            _photonView.RequestOwnership();
            */

            if (_photonView.IsMine) return;
            if (!_myPhotonView.IsMine) return;
            if (!canChange) return;
            //if (_photonView.Owner == gameObject.GetComponent<PhotonView>().Owner) return;
            // 所有権の移譲
            ////////other.gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);

            //other.gameObject.GetComponent<BallDebug>().ChangeOwner(PhotonNetwork.LocalPlayer);

            _ballDebug.ChangeOwner(PhotonNetwork.LocalPlayer);

            Debug.Log(PhotonNetwork.LocalPlayer);
            canChange = false;
            //PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
        }
    }
}
