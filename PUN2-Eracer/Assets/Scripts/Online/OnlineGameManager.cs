using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    private Vector3 pos;
    private Quaternion rotate;

    // Start is called before the first frame update
    void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pos = new Vector3(75, 9.5f, 0);
            rotate = Quaternion.EulerAngles(0, 45, 0);
        }
        else
        {
            pos = new Vector3(125, 9.5f, 60);
            rotate = Quaternion.EulerAngles(0, 225, 0);
        }
        PhotonNetwork.Instantiate("RocketCar1v1", pos, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
