using System;
using System.Collections.Generic;
using UnityEngine;
using ShootingGame.Core;
using ShootingGame.Bullet;
using ShootingGame.Enemy;

namespace ShootingGame.Boss
{
    public enum BossState { Entering, Battle, Dead }

    /// <summary>
    /// 보스 본체(코어). 등장→전투의 FSM, 코어 체력 구간별 페이즈 전환, 페이즈별 이동·탄막,
    /// 파괴 가능한 파츠 생성/관리, 사망 처리. 코어는 IDamageable(중앙 약점). (GDD §5.2)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Boss : MonoBehaviour, IDamageable
    {
        BossData data;
        SpriteRenderer body;
        Material mat;
        Transform player;

        BossState state;
        float coreHp, maxCoreHp;
        int phaseIndex = -1;
        float fireTimer, battleTime;
        bool transformed;
        float invulnTimer;
        readonly List<BossPart> parts = new List<BossPart>();

        public Action Defeated;

        public string BossName => data != null ? data.bossName : "";
        public float CoreHpRatio => maxCoreHp > 0f ? Mathf.Clamp01(coreHp / maxCoreHp) : 0f;

        public Transform Transform => transform;
        public float HitRadius => data != null ? data.coreHitRadius : 0.7f;
        public bool IsDead => state == BossState.Dead;

        public void Setup(BossData d, Vector3 spawnPos, Material spriteMat)
        {
            data = d;
            mat = spriteMat;
            transform.position = spawnPos;
            transform.localScale = Vector3.one * d.bodyScale;
            coreHp = maxCoreHp = d.coreHp;
            state = BossState.Entering;
            phaseIndex = -1;

            var pgo = GameObject.FindWithTag("Player");
            player = pgo != null ? pgo.transform : null;

            body = GetComponent<SpriteRenderer>();
            body.sprite = d.bodySprite;
            body.color = d.bodyColor;
            body.sortingOrder = 11;
            if (mat != null) body.sharedMaterial = mat;

            if (d.parts != null)
            {
                foreach (var pd in d.parts)
                {
                    var go = new GameObject("Part_" + pd.name);
                    go.transform.SetParent(transform, false);
                    // 부모 스케일 보정: 파츠 localScale은 Setup에서 절대값으로 덮으므로 부모 스케일 영향 받음.
                    var part = go.AddComponent<BossPart>();
                    part.Destroyed = OnPartDestroyed;
                    part.Setup(pd, player, mat);
                    parts.Add(part);
                }
            }

            if (CollisionManager.Instance != null) CollisionManager.Instance.RegisterTarget(this);
            if (GameManager.Instance != null) GameManager.Instance.NotifyBossSpawned(d.bossName);
        }

        void OnPartDestroyed(BossPart p) => parts.Remove(p);

        void Update()
        {
            if (invulnTimer > 0f)
            {
                invulnTimer -= Time.deltaTime;
                Color c = body.color;
                c.a = invulnTimer > 0f && Mathf.FloorToInt(Time.unscaledTime * 12f) % 2 == 0 ? 0.4f : 1f;
                body.color = c;
            }

            switch (state)
            {
                case BossState.Entering:
                    transform.position += Vector3.down * (data.entrySpeed * Time.deltaTime);
                    if (transform.position.y <= data.battleY)
                    {
                        var p = transform.position; p.y = data.battleY; transform.position = p;
                        state = BossState.Battle;
                        battleTime = 0f;
                        UpdatePhase(true);
                    }
                    break;

                case BossState.Battle:
                    battleTime += Time.deltaTime;
                    UpdatePhase(false);
                    Move();
                    Fire();
                    break;
            }
        }

        void UpdatePhase(bool force)
        {
            if (data.phases == null || data.phases.Length == 0) return;
            float ratio = CoreHpRatio;
            int idx = 0;
            for (int i = 0; i < data.phases.Length; i++)
                if (data.phases[i].hpThreshold >= ratio) idx = i;
            if (idx != phaseIndex || force)
            {
                phaseIndex = idx;
                fireTimer = data.phases[idx].fireInterval;
            }
        }

        BossPhaseDef Phase => data.phases[Mathf.Clamp(phaseIndex, 0, data.phases.Length - 1)];

        void Move()
        {
            var ph = Phase;
            var pos = transform.position;
            pos.x = Mathf.Sin(battleTime * ph.moveSpeed) * ph.moveAmplitudeX;
            transform.position = pos;
        }

        void Fire()
        {
            if (BulletPool.Instance == null || data.phases == null || data.phases.Length == 0) return;
            var ph = Phase;
            fireTimer -= Time.deltaTime;
            if (fireTimer > 0f) return;
            fireTimer = ph.fireInterval;
            FirePattern(ph);
        }

        void FirePattern(BossPhaseDef ph)
        {
            Vector2 origin = transform.position;
            switch (ph.pattern)
            {
                case BossPatternType.Aimed:
                    Shoot(origin, AimDir(origin), ph);
                    break;

                case BossPatternType.NWay:
                {
                    float baseAng = Mathf.Atan2(AimDir(origin).y, AimDir(origin).x) * Mathf.Rad2Deg;
                    int n = Mathf.Max(1, ph.wayCount);
                    float half = ph.spreadAngle * 0.5f;
                    for (int i = 0; i < n; i++)
                    {
                        float a = n == 1 ? baseAng : baseAng - half + ph.spreadAngle * i / (n - 1);
                        Shoot(origin, AngleDir(a), ph);
                    }
                    break;
                }

                case BossPatternType.Circle:
                {
                    int n = Mathf.Max(1, ph.wayCount);
                    for (int i = 0; i < n; i++)
                        Shoot(origin, AngleDir(360f * i / n), ph);
                    break;
                }
            }
        }

        Vector2 AimDir(Vector2 origin) =>
            player != null ? ((Vector2)player.position - origin).normalized : Vector2.down;

        static Vector2 AngleDir(float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
        }

        void Shoot(Vector2 origin, Vector2 dir, BossPhaseDef ph)
        {
            var b = BulletPool.Instance.Get();
            b.Launch(origin, dir * ph.bulletSpeed, ph.bulletDamage, ph.bulletRadius,
                     false, false, ph.bulletColor, null);
        }

        public void TakeDamage(float amount)
        {
            if (state == BossState.Dead || invulnTimer > 0f) return;
            coreHp -= amount;
            if (AudioManager.Instance != null) AudioManager.Instance.Play("hit", 0.18f, 0.1f);
            if (GameManager.Instance != null) GameManager.Instance.NotifyBossHp(CoreHpRatio);
            if (coreHp <= 0f) { Die(); return; }
            CheckTransform();
        }

        void CheckTransform()
        {
            if (!data.useTransform || transformed) return;
            if (CoreHpRatio > data.transformAtHp) return;
            transformed = true;
            invulnTimer = data.transformInvulnTime;
            if (data.transformSprite != null) body.sprite = data.transformSprite;
            body.color = data.transformColor;
        }

        void Die()
        {
            state = BossState.Dead;
            if (CollisionManager.Instance != null) CollisionManager.Instance.UnregisterTarget(this);
            foreach (var p in parts) if (p != null) p.ForceRemove();
            parts.Clear();

            // 분리: 잔해 적 N기 방사
            if (data.separateOnDeath && data.separatedEnemy != null && EnemyPool.Instance != null)
            {
                int n = Mathf.Max(1, data.separatedCount);
                for (int i = 0; i < n; i++)
                {
                    float a = 360f * i / n * Mathf.Deg2Rad;
                    Vector3 off = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * (data.coreHitRadius + 0.4f);
                    EnemyPool.Instance.Spawn(data.separatedEnemy, transform.position + off);
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(data.score);
                GameManager.Instance.NotifyBossDefeated();
            }
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.5f, 0.6f);
            if (AudioManager.Instance != null) AudioManager.Instance.Play("bossexp", 0.9f);
            if (EffectPool.Instance != null)
                for (int i = 0; i < 6; i++)
                    EffectPool.Instance.Play(transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 1.3f),
                                             1.1f, new Color(1f, 0.6f, 0.2f, 1f));

            Defeated?.Invoke();
            Destroy(gameObject);
        }
    }
}
