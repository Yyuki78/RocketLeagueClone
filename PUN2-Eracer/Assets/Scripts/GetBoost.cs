using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GetBoost : MonoBehaviourPunCallbacks
{
    //小ブーストか大ブーストか
    [SerializeField] int BoostObjType = 0;

    private bool enable = true;

    private MeshRenderer _material;
    [SerializeField] Material _white;
    [SerializeField] Material _yellow;

    [SerializeField] GameObject _MaxBoostBall;

    private AudioSource _audio;
    [SerializeField] AudioClip _clip;

    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<MeshRenderer>();
        _material.material = _yellow;
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enable) return;
        if (other.gameObject.layer == 8)
        {
            var move = other.gameObject.GetComponentInParent<CarMove3>();
            if (move.BoostQuantity >= 100) return;

            if (move.IsOnline)
            {
                _audio.PlayOneShot(_clip);
                if (BoostObjType == 1)
                {
                    move.GetBoostMini();
                }
                else
                {
                    move.GetBoostMax();
                }
                photonView.RPC(nameof(RpcGetBoost), RpcTarget.All);
            }
            else
            {
                _audio.PlayOneShot(_clip);
                if (BoostObjType == 1)
                {
                    move.GetBoostMini();
                }
                else
                {
                    move.GetBoostMax();
                }
                enable = false;
                StartCoroutine(CoolDown());
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!enable) return;
        if (other.gameObject.layer == 8)
        {
            var move = other.gameObject.GetComponentInParent<CarMove3>();
            if (move.BoostQuantity >= 100) return;

            if (move.IsOnline)
            {
                _audio.PlayOneShot(_clip);
                if (BoostObjType == 1)
                {
                    move.GetBoostMini();
                }
                else
                {
                    move.GetBoostMax();
                }
                photonView.RPC(nameof(RpcGetBoost), RpcTarget.All);
            }
            else
            {
                _audio.PlayOneShot(_clip);
                if (BoostObjType == 1)
                {
                    move.GetBoostMini();
                }
                else
                {
                    move.GetBoostMax();
                }
                enable = false;
                StartCoroutine(CoolDown());
            }
        }
    }

    private IEnumerator CoolDown()
    {
        if (enable) yield break;
        _material.material = _white;
        if (BoostObjType == 1)
        {
            yield return new WaitForSeconds(4f);
            if (enable) yield break;
        }
        else
        {
            _MaxBoostBall.SetActive(false);
            yield return new WaitForSeconds(10f);
            if (enable) yield break;
            _MaxBoostBall.SetActive(true);
        }
        _material.material = _yellow;
        enable = true;
        yield break;
    }

    [PunRPC]
    private void RpcGetBoost()
    {
        enable = false;
        StartCoroutine(CoolDown());
    }

    public void Reset()
    {
        Debug.Log("ブーストリセット");
        if (BoostObjType != 1)
        {
            _MaxBoostBall.SetActive(true);
            Debug.Log("大ブーストリセット");
        }
        _material.material = _yellow;
        enable = true;
    }
}
