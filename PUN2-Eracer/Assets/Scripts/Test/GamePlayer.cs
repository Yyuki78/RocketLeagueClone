using Photon.Pun;
using UnityEngine;

public class GamePlayer : MonoBehaviourPunCallbacks
{
    private ProjectileManager projectileManager;
    // 弾を発射する時に使う弾のID
    private int projectileId = 0;

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

                photonView.RPC(nameof(FireProjectile), RpcTarget.All, transform.position, angle);
            }
        }
    }

    [PunRPC]
    private void FireProjectile(Vector3 origin, float angle, PhotonMessageInfo info)
    {
        int timestamp = info.SentServerTimestamp;
        projectileManager.Fire(timestamp, photonView.OwnerActorNr, origin, angle, timestamp);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine)
        {
            var projectile = collision.GetComponent<Projectile>();
            if (projectile != null && projectile.OwnerId != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                photonView.RPC(nameof(HitByProjectile), RpcTarget.All, projectile.Id, projectile.OwnerId);
            }
        }
    }

    [PunRPC]
    private void HitByProjectile(int projectileId, int ownerId)
    {
        projectileManager.Remove(projectileId, ownerId);
    }
}