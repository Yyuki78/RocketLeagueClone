using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    private Vector3 pos;
    private Quaternion rotate;

    private bool once = true;

    private GameObject myCar;

    private ChangeCarColor _changeColor;

    // Start is called before the first frame update
    void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pos = new Vector3(75, 9.5f, 0);
            rotate = Quaternion.EulerAngles(0, 45, 0);
            PhotonNetwork.NickName = "1P";
        }
        else
        {
            pos = new Vector3(125, 9.5f, 60);
            rotate = Quaternion.EulerAngles(0, 180, 0);
            PhotonNetwork.NickName = "2P";
        }
        myCar = PhotonNetwork.Instantiate("RocketCar1v1", pos, rotate);
    }

    // Update is called once per frame
    void Update()
    {
        if (once)
        {
            once = false;
            if (!PhotonNetwork.IsMasterClient)
            {
                
            }
        }
    }
}
