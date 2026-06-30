using UnityEngine;
using ShootingGame.Weapon;

namespace ShootingGame.Item
{
    /// <summary>
    /// 필드의 무기 아이템(W). 기체가 픽업 반경에 닿으면 해당 무기로 즉시 교체된다. (GDD §3 — 즉시 교체형)
    /// 충돌은 원-원 거리 비교(CLAUDE.md §2)로 직접 판정.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class WeaponPickup : MonoBehaviour
    {
        [Tooltip("이 픽업이 부여할 무기 카드")]
        [SerializeField] WeaponData weapon;
        [Tooltip("획득 반경(월드 유닛)")]
        [SerializeField] float pickupRadius = 0.7f;
        [Tooltip("아이템 하강 속도(스크롤 연출). 0이면 정지")]
        [SerializeField] float fallSpeed = 0f;

        WeaponController target;
        static Camera cam;

        /// <summary>드롭 생성 시 부여할 무기를 지정(DropManager에서 호출).</summary>
        public void SetWeapon(WeaponData w) => weapon = w;

        void Awake()
        {
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
                target = playerGo.GetComponent<WeaponController>();
            if (cam == null) cam = Camera.main;
        }

        void Update()
        {
            if (fallSpeed != 0f)
                transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

            if (target != null && weapon != null)
            {
                float sqrDist = ((Vector2)target.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (sqrDist <= pickupRadius * pickupRadius)
                {
                    target.Equip(weapon);
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}
