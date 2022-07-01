using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetBoost : MonoBehaviour
{
    //小ブーストか大ブーストか
    [SerializeField] int BoostObjType = 0;

    private bool enable = true;

    private MeshRenderer _material;
    [SerializeField] Material _white;
    [SerializeField] Material _yellow;

    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<MeshRenderer>();
        _material.material = _yellow;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enable) return;
        if (other.gameObject.layer == 8)
        {
            enable = false;
            var move = other.gameObject.GetComponentInParent<CarMove3>();

            if (BoostObjType == 1)
            {
                move.GetBoostMini();
            }
            else
            {
                move.GetBoostMax();
            }
            StartCoroutine(CoolDown());
        }
    }

    private IEnumerator CoolDown()
    {
        if (enable) yield break;
        _material.material = _white;
        if (BoostObjType == 1)
        {
            yield return new WaitForSeconds(4f);
        }
        else
        {
            yield return new WaitForSeconds(10f);
        }
        _material.material = _yellow;
        enable = true;
        yield break;
    }
}
