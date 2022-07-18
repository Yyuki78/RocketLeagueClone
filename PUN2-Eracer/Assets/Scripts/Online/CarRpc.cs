using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CarRpc : MonoBehaviourPunCallbacks
{
    private int startPos;
    private Vector3 pos;
    private Quaternion rotate;

    private PhotonView _view;
    private CarMove3 _move;

    // Start is called before the first frame update
    void Start()
    {
        _view = GetComponent<PhotonView>();
        _move = GetComponent<CarMove3>();

        if (_view.IsMine)
        {
            StartCoroutine(SetStartPos());
        }
    }

    // このメソッドで受信する
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        object value = null;
        if (propertiesThatChanged.TryGetValue("startPos", out value))
        {
            RpcSetPosition((int)value);
        }
    }

    private void RpcSetPosition(int StartPos)
    {
        switch (StartPos)
        {
            case 1:
                if (PhotonNetwork.IsMasterClient)
                {
                    pos = new Vector3(77.5f, 9.75f, 2.5f);
                    rotate = Quaternion.Euler(0, 45, 0);
                }
                else
                {
                    pos = new Vector3(122.5f, 10f, 57.5f);
                    rotate = Quaternion.Euler(0, 225, 0);
                }
                break;
            case 2:
                if (PhotonNetwork.IsMasterClient)
                {
                    pos = new Vector3(100, 10f, -20);
                    rotate = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    pos = new Vector3(100, 10f, 80);
                    rotate = Quaternion.Euler(0, 180, 0);
                }
                break;
            case 3:
                if (PhotonNetwork.IsMasterClient)
                {
                    pos = new Vector3(122.5f, 10f, 2.5f);
                    rotate = Quaternion.Euler(0, -45, 0);
                }
                else
                {
                    pos = new Vector3(77.5f, 10f, 57.5f);
                    rotate = Quaternion.Euler(0, -225, 0);
                }
                break;
            case 4:
                if (PhotonNetwork.IsMasterClient)
                {
                    pos = new Vector3(97, 10f, -10);
                    rotate = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    pos = new Vector3(103, 10f, 70);
                    rotate = Quaternion.Euler(0, 180, 0);
                }
                break;
            case 5:
                if (PhotonNetwork.IsMasterClient)
                {
                    pos = new Vector3(103, 10f, -10);
                    rotate = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    pos = new Vector3(97, 10f, 70);
                    rotate = Quaternion.Euler(0, 180, 0);
                }
                break;
            default:
                Debug.Log("スタートポジションセットミス");
                break;
        }
    }

    private IEnumerator SetStartPos()
    {
        if (PhotonNetwork.IsMasterClient && _view.IsMine)
        {
            // HashTable型であるカスタムプロパティを作成
            var properties = new ExitGames.Client.Photon.Hashtable();
            // 同期させたい共通変数をHashTableに追加する
            properties.Add("startPos", (int)Random.Range(1, 6));
            // このメソッドで送信する
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }
        yield return new WaitForSeconds(0.3f);

        _move.Respown();
        transform.position = pos;
        transform.rotation = rotate;
        yield break;
    }

    public void Respawn()
    {
        StartCoroutine(Restart());
    }

    private IEnumerator Restart()
    {
        _move.isMoving = false;
        if (_view.IsMine)
        {
            StartCoroutine(SetStartPos());
        }
        yield return new WaitForSeconds(3.5f);
        _move.isMoving = true;
    }
}
