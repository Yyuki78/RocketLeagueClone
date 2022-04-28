using Photon.Pun;
using UnityEngine;

public class GamePlayer : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Projectile projectilePrefab = default;

    private ProjectileManager projectileManager;

    private void Awake()
    {
        projectileManager = GameObject.FindWithTag("ProjectileManager").GetComponent<ProjectileManager>();
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            var direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            var dv = 6f * Time.deltaTime * direction;
            transform.Translate(dv.x, dv.y, 0f);

            if (Input.GetMouseButtonDown(0))
            {
                var playerWorldPosition = transform.position;
                var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var dp = mouseWorldPosition - playerWorldPosition;
                float angle = Mathf.Atan2(dp.y, dp.x);

                // FireProjectile(angle)をRPCで実行する
                photonView.RPC(nameof(FireProjectile), RpcTarget.All, angle);
            }
        }
    }

    // [PunRPC]属性をつけると、RPCでの実行が有効になる
    [PunRPC]
    private void FireProjectile(float angle)
    {
        projectileManager.Fire(transform.position, angle);
    }
}