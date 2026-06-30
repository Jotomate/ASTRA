using UnityEngine;
using PlayerShip = ShootingGame.Player.Player;

namespace ShootingGame.Item
{
    /// <summary>
    /// 파워업 아이템(P). 기체가 닿으면 무기-출력 +1단계. (GDD §3)
    /// 적 처치 드롭으로 생성되어 아래로 흘러내린다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PowerUpPickup : MonoBehaviour
    {
        [Min(1)][SerializeField] int amount = 1;
        [SerializeField] float pickupRadius = 0.7f;
        [SerializeField] float fallSpeed = 1.5f;

        PlayerShip player;
        static Camera cam;

        void Awake()
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo != null) player = pgo.GetComponent<PlayerShip>();
            if (cam == null) cam = Camera.main;
        }

        void Update()
        {
            if (fallSpeed != 0f)
                transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

            if (player != null && player.isActiveAndEnabled)
            {
                float sqr = ((Vector2)player.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (sqr <= pickupRadius * pickupRadius)
                {
                    player.AddPower(amount);
                    if (ShootingGame.Core.AudioManager.Instance != null)
                        ShootingGame.Core.AudioManager.Instance.Play("power", 0.7f);
                    Destroy(gameObject);
                    return;
                }
            }

            if (IsOffscreenBottom()) Destroy(gameObject);
        }

        bool IsOffscreenBottom()
        {
            if (cam == null || !cam.orthographic) return false;
            return transform.position.y < cam.transform.position.y - cam.orthographicSize - 1f;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}
