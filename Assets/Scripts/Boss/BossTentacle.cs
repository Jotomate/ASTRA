using UnityEngine;
using PlayerShip = ShootingGame.Player.Player;

namespace ShootingGame.Boss
{
    /// <summary>
    /// 보스 촉수(§5.14). FABRIK 분절 체인 + 공격 상태머신:
    /// Idle(꿈틀거림) → Windup(코일·예고) → Strike(빠른 타격) → Recover(복귀).
    /// 타격 중 팁이 기체에 닿으면 피해. 보스 소멸 시 자동 정리.
    /// </summary>
    public class BossTentacle : MonoBehaviour
    {
        enum TState { Idle, Windup, Strike, Recover }

        Transform[] seg;
        SpriteRenderer[] segSr;
        int n;
        float spacing, reach, speed, hitRadius, dmgCd;
        Transform boss, player;
        Vector3 rootLocal, tip, strikeTarget;
        Color baseColor;
        float sideSign;

        TState st;
        float stTimer, phase;

        const float IDLE = 1.8f, WINDUP = 0.5f, STRIKE = 0.2f, RECOVER = 0.6f;
        const float SWAY_AMP = 0.75f, SWAY_SPEED = 2.4f;
        const float STRIKE_SPEED = 20f, WINDUP_SPEED = 7f, RECOVER_SPEED = 5f;

        public void Setup(Transform bossT, Vector3 worldOffset, int segments, float spacing,
                          float reach, float speed, Sprite sprite, Material mat, Color color, float phaseOffset)
        {
            boss = bossT; rootLocal = worldOffset; n = Mathf.Max(2, segments);
            this.spacing = spacing; this.reach = reach; this.speed = speed; hitRadius = 0.3f;
            baseColor = color;
            sideSign = Mathf.Sign(worldOffset.x == 0f ? 1f : worldOffset.x);
            phase = phaseOffset;

            seg = new Transform[n];
            segSr = new SpriteRenderer[n];
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject("Seg" + i);
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite; sr.color = color; sr.sortingOrder = 10;
                if (mat != null) sr.sharedMaterial = mat;
                go.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.45f, (float)i / (n - 1));
                seg[i] = go.transform; segSr[i] = sr;
            }
            var pgo = GameObject.FindWithTag("Player");
            player = pgo != null ? pgo.transform : null;

            Vector3 root = boss.position + rootLocal;
            for (int i = 0; i < n; i++) seg[i].position = root + Vector3.down * (spacing * i);
            tip = seg[n - 1].position;

            st = TState.Idle;
            stTimer = IDLE + phaseOffset;   // 촉수별 스태거
        }

        void Update()
        {
            if (boss == null) { Destroy(gameObject); return; }
            float dt = Time.deltaTime;
            Vector3 root = boss.position + rootLocal;
            // 평상시 쉬는 지점: 자기 쪽으로 벌려서 아래
            Vector3 hang = root + new Vector3(sideSign * reach * 0.5f, -reach * 0.55f, 0f);

            stTimer -= dt;
            Vector3 desired = hang;
            Color tint = baseColor;
            float sp = speed * 1.5f;

            switch (st)
            {
                case TState.Idle:
                    float w = (Time.time + phase) * SWAY_SPEED;
                    desired = hang + new Vector3(Mathf.Cos(w) * SWAY_AMP, Mathf.Sin(w * 1.4f) * SWAY_AMP * 0.6f, 0f);
                    if (stTimer <= 0f)
                    {
                        st = TState.Windup; stTimer = WINDUP;
                        strikeTarget = player != null
                            ? root + (Vector3)Vector2.ClampMagnitude((Vector2)player.position - (Vector2)root, reach)
                            : root + Vector3.down * reach;
                    }
                    break;

                case TState.Windup:
                    desired = Vector3.Lerp(root, hang, 0.2f);   // 코일(움츠림)
                    tint = Color.Lerp(baseColor, new Color(1f, 0.5f, 0.2f), 0.8f);   // 주황 예고
                    sp = WINDUP_SPEED;
                    if (stTimer <= 0f) { st = TState.Strike; stTimer = STRIKE; }
                    break;

                case TState.Strike:
                    desired = strikeTarget;                      // 빠른 타격
                    tint = new Color(1f, 0.85f, 0.95f);
                    sp = STRIKE_SPEED;
                    if (stTimer <= 0f) { st = TState.Recover; stTimer = RECOVER; }
                    break;

                case TState.Recover:
                    desired = hang;
                    sp = RECOVER_SPEED;
                    if (stTimer <= 0f) { st = TState.Idle; stTimer = IDLE; }
                    break;
            }

            tip = Vector3.MoveTowards(tip, desired, sp * dt);

            // FABRIK 1패스
            seg[n - 1].position = tip;
            for (int i = n - 2; i >= 0; i--)
            {
                Vector3 d = seg[i].position - seg[i + 1].position;
                if (d.sqrMagnitude < 1e-4f) d = Vector3.up;
                seg[i].position = seg[i + 1].position + d.normalized * spacing;
            }
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
                segSr[i].color = Color.Lerp(segSr[i].color, tint, dt * 12f);
            }

            // 타격 중 팁 접촉 피해
            if (dmgCd > 0f) dmgCd -= dt;
            if (player != null && dmgCd <= 0f && (st == TState.Strike || st == TState.Windup))
            {
                var ps = player.GetComponent<PlayerShip>();
                if (ps != null && !ps.IsInvulnerable)
                {
                    float r = hitRadius + ps.HitboxRadius;
                    if (((Vector2)tip - (Vector2)player.position).sqrMagnitude <= r * r)
                        if (ps.TryHit()) dmgCd = 0.6f;
                }
            }
        }
    }
}
