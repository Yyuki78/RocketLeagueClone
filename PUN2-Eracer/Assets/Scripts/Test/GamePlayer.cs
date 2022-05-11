using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(SpriteRenderer))]
public class GamePlayer : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TextMeshPro nameLabel = default;

    private ProjectileManager projectileManager;
    private SpriteRenderer spriteRenderer;

    public Player Owner => photonView.Owner;

    private void Awake()
    {
        projectileManager = GameObject.FindWithTag("ProjectileManager").GetComponent<ProjectileManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        var gamePlayerManager = GameObject.FindWithTag("GamePlayerManager").GetComponent<GamePlayerManager>();
        transform.SetParent(gamePlayerManager.transform);
    }

    private void Start()
    {
        // プレイヤー名の横にスコアを表示する
        int score = photonView.Owner.GetScore();
        nameLabel.text = $"{photonView.Owner.NickName}({score.ToString()})";

        // 色相値が設定されていたら、スプライトの色を変化させる
        if (photonView.Owner.TryGetHue(out float hue))
        {
            spriteRenderer.color = Color.HSVToRGB(hue, 1f, 1f);
        }
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

        if (photonView.IsMine)
        {
            PhotonNetwork.LocalPlayer.OnTakeDamage();
        }
        else if (ownerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PhotonNetwork.LocalPlayer.OnDealDamage();
        }
    }

    // プレイヤーのカスタムプロパティが更新された時に呼ばれるコールバック
    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        if (target.ActorNumber != photonView.OwnerActorNr) { return; }

        // スコアが更新されていたら、スコア表示も更新する
        if (changedProps.TryGetScore(out int score))
        {
            nameLabel.text = $"{photonView.Owner.NickName}({score.ToString()})";
        }

        // 色相値が更新されていたら、スプライトの色を変化させる
        if (changedProps.TryGetHue(out float hue))
        {
            spriteRenderer.color = Color.HSVToRGB(hue, 1f, 1f);
        }
    }
}