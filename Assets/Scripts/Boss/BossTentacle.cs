using UnityEngine;
using PlayerShip = ShootingGame.Player.Player;

namespace ShootingGame.Boss
{
    /// <summary>
    /// 보스 촉수(§5.14 대표 구현). 분절 체인이 플레이어를 향해 휘며 추적(FABRIK 1패스),
    /// 끝(팁)이 기체에 닿으면 피해. 보스가 사라지면 자동 정리.
    /// ※ 완전한 다관절 IK 무기화는 후속.
    /// </summary>
    public class BossTentacle : MonoBehaviour
    {
        Transform[] seg;
        int n;
        float spacing, reach, speed, hitRadius, dmgCd;
        Transform boss, player;
        Vector3 rootLocal, tip;

        public void Setup(Transform bossT, Vector3 worldOffset, int segments, float spacing,
                          float reach, float speed, Sprite sprite, Material mat, Color color)
        {
            boss = bossT; rootLocal = worldOffset; n = Mathf.Max(2, segments);
            this.spacing = spacing; this.reach = reach; this.speed = speed; hitRadius = 0.28f;

            seg = new Transform[n];
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject("Seg" + i);
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite; sr.color = color; sr.sortingOrder = 10;
                if (mat != null) sr.sharedMaterial = mat;
                go.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.45f, (float)i / (n - 1));
                seg[i] = go.transform;
            }
            var pgo = GameObject.FindWithTag("Player");
            player = pgo != null ? pgo.transform : null;
            Vector3 root = boss.position + rootLocal;
            for (int i = 0; i < n; i++) seg[i].position = root + Vector3.down * (spacing * i);
            tip = seg[n - 1].position;
        }

        void Update()
        {
            if (boss == null) { Destroy(gameObject); return; }   // 보스 소멸 시 정리
            float dt = Time.deltaTime;
            Vector3 root = boss.position + rootLocal;

            Vector3 target = player != null
                ? root + Vector3.ClampMagnitude(player.position - root, reach)
                : root + Vector3.down * reach;
            tip = Vector3.MoveTowards(tip, target, speed * dt);

            // FABRIK: 뒤로(팁→루트)
            seg[n - 1].position = tip;
            for (int i = n - 2; i >= 0; i--)
            {
                Vector3 d = seg[i].position - seg[i + 1].position;
                if (d.sqrMagnitude < 1e-4f) d = Vector3.up;
                seg[i].position = seg[i + 1].position + d.normalized * spacing;
            }
            // 앞으로(루트 고정)
            seg[0].position = root;
            for (int i = 1; i < n; i++)
            {
                Vector3 d = seg[i].position - seg[i - 1].position;
                if (d.sqrMagnitude < 1e-4f) d = Vector3.down;
                seg[i].position = seg[i - 1].position + d.normalized * spacing;
            }
            tip = seg[n - 1].position;

            for (int i = 0; i < n; i++)
            {
                Vector3 dir = i < n - 1 ? seg[i + 1].position - seg[i].position : seg[i].position - seg[i - 1].position;
                float a = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                seg[i].rotation = Quaternion.Euler(0, 0, a);
            }

            // 팁 접촉 데미지
            if (dmgCd > 0f) dmgCd -= dt;
            if (player != null && dmgCd <= 0f)
            {
                var ps = player.GetComponent<PlayerShip>();
                if (ps != null && !ps.IsInvulnerable)
                {
                    float r = hitRadius + ps.HitboxRadius;
                    if (((Vector2)tip - (Vector2)player.position).sqrMagnitude <= r * r)
                        if (ps.TryHit()) dmgCd = 0.5f;
                }
            }
        }
    }
}
