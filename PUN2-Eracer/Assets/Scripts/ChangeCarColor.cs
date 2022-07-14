using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ChangeCarColor : MonoBehaviourPunCallbacks
{
    public bool isChange = false;

    private MeshRenderer _mesh;
    [SerializeField] Material Red;

    private bool once = true;

    // Start is called before the first frame update
    void Start()
    {
        var view = gameObject.GetComponentInParent<PhotonView>();
        _mesh = GetComponent<MeshRenderer>();
        if (!PhotonNetwork.IsMasterClient)
        {
            if (view.IsMine)
            {
                Material[] mats = _mesh.materials;
                mats[0] = Red;
                mats[1] = Red;
                _mesh.materials = mats;
            }
        }
        else
        {
            if (!view.IsMine)
            {
                Material[] mats = _mesh.materials;
                mats[0] = Red;
                mats[1] = Red;
                _mesh.materials = mats;
            }
        }
    }
}
