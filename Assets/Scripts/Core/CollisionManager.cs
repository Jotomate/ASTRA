using System.Collections.Generic;
using UnityEngine;
using ShootingGame.Bullet;
using PlayerShip = ShootingGame.Player.Player;

namespace ShootingGame.Core
{
    /// <summary>
    /// 중앙 충돌 판정(원-원 거리 비교, CLAUDE.md §2). 매 프레임 다음 쌍을 검사한다:
    ///   ① 자기탄 → 피격대상(IDamageable)   ② 적탄 → 기체   ③ 대상 본체 → 기체.
    /// 대상(적·보스 파츠)은 스폰/소멸 시 등록/해제, 탄은 BulletPool.Active를 순회한다.
    /// 이동 이후 판정을 위해 LateUpdate에서 수행.
    /// </summary>
    public class CollisionManager : MonoBehaviour
    {
        public static CollisionManager Instance { get; private set; }

        readonly List<IDamageable> targets = new List<IDamageable>(128);
        PlayerShip player;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void RegisterTarget(IDamageable t)
        {
            if (t != null && !targets.Contains(t)) targets.Add(t);
        }

        public void UnregisterTarget(IDamageable t)
        {
            targets.Remove(t);
        }

        void EnsurePlayer()
        {
            if (player != null && player.isActiveAndEnabled) return;
            var pgo = GameObject.FindWithTag("Player");
            player = pgo != null ? pgo.GetComponent<PlayerShip>() : null;
        }

        void LateUpdate()
        {
            EnsurePlayer();

            var bp = BulletPool.Instance;
            var bullets = bp != null ? bp.Active : null;

            // ① 자기탄 → 대상,  ② 적탄 → 기체  (탄을 역방향 순회: Despawn 시 안전)
            if (bullets != null)
            {
                for (int bi = bullets.Count - 1; bi >= 0; bi--)
                {
                    var b = bullets[bi];
                    if (b == null || !b.gameObject.activeSelf) continue;
                    Vector2 bpos = b.transform.position;

                    if (b.IsPlayerBullet)
                    {
                        for (int ti = targets.Count - 1; ti >= 0; ti--)
                        {
                            var t = targets[ti];
                            if (t == null || t.IsDead) continue;
                            if (Overlap(bpos, b.Radius, t.Transform.position, t.HitRadius))
                            {
                                t.TakeDamage(b.Damage);
                                if (!b.Piercing) { b.Despawn(); break; }
                                // 관통탄: break 없이 계속 진행 → 겹친 다른 대상도 동시 타격
                            }
                        }
                    }
                    else // 적탄 → 기체
                    {
                        if (player != null && !player.IsInvulnerable
                            && Overlap(bpos, b.Radius, player.transform.position, player.HitboxRadius))
                        {
                            player.TryHit();
                            b.Despawn();
                        }
                    }
                }
            }

            // ③ 대상 본체 → 기체
            if (player != null && !player.IsInvulnerable)
            {
                for (int ti = targets.Count - 1; ti >= 0; ti--)
                {
                    var t = targets[ti];
                    if (t == null || t.IsDead) continue;
                    if (Overlap(player.transform.position, player.HitboxRadius, t.Transform.position, t.HitRadius))
                    {
                        player.TryHit();
                        break;
                    }
                }
            }
        }

        /// <summary>이탈 폭발 등 AoE: 반경 내 대상에 데미지 + 반경 내 적탄 소거.</summary>
        public int BlastRadius(Vector2 center, float radius, float damage)
        {
            int hitCount = 0;
            for (int ti = targets.Count - 1; ti >= 0; ti--)
            {
                var t = targets[ti];
                if (t == null || t.IsDead) continue;
                if (Overlap(center, radius, t.Transform.position, t.HitRadius))
                {
                    t.TakeDamage(damage);
                    hitCount++;
                }
            }

            var bp = BulletPool.Instance;
            if (bp != null)
            {
                var bullets = bp.Active;
                for (int bi = bullets.Count - 1; bi >= 0; bi--)
                {
                    var b = bullets[bi];
                    if (b == null || b.IsPlayerBullet) continue;
                    if (Overlap(center, radius, b.transform.position, b.Radius))
                        b.Despawn();
                }
            }
            return hitCount;
        }

        /// <summary>유도탄용 최근접 대상. 자기탄이면 가장 가까운 적/보스, 적탄이면 기체.</summary>
        public Transform FindNearestTarget(Vector2 from, bool forPlayerBullet)
        {
            if (!forPlayerBullet)
            {
                EnsurePlayer();
                return player != null ? player.transform : null;
            }
            Transform best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null || t.IsDead) continue;
                float d = ((Vector2)t.Transform.position - from).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = t.Transform; }
            }
            return best;
        }

        static bool Overlap(Vector2 a, float ar, Vector2 b, float br)
        {
            float r = ar + br;
            return (a - b).sqrMagnitude <= r * r;
        }
    }
}
