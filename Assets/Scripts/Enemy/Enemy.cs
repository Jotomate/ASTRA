using UnityEngine;
using ShootingGame.Bullet;
using ShootingGame.Core;
using PlayerShip = ShootingGame.Player.Player;

namespace ShootingGame.Enemy
{
    public enum EnemyState { Appear, Act, Dead }

    /// <summary>
    /// 데이터 주도 적 개체. EnemyData를 읽어 등장(페이드)→행동(이동·발사)→소멸의 간이 FSM으로 작동한다.
    /// 충돌은 CollisionManager(원-원)가 담당. 풀링됨.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Enemy : MonoBehaviour, IDamageable
    {
        EnemyData data;
        SpriteRenderer sr;
        EnemyPool pool;
        Transform player;

        EnemyState state;
        float hp;
        float appearTimer;
        float fireTimer;
        float flashTimer;
        bool scattering;
        Vector2 scatterDir;
        Vector2 facing = Vector2.down;
        Vector2 spawnOrigin;
        int pathIndex;
        static Camera cam;

        public EnemyData Data => data;
        public float Radius => data != null ? data.hitboxRadius : 0.3f;
        public bool IsDead => state == EnemyState.Dead;
        public bool IsActing => state == EnemyState.Act;
        /// <summary>격파(피격 사망) 시 호출. 화면밖 소멸에는 호출되지 않음(편대 전멸 판정용).</summary>
        public System.Action<Enemy> OnKilled;

        // IDamageable
        public Transform Transform => transform;
        public float HitRadius => Radius;

        void Awake() => sr = GetComponent<SpriteRenderer>();
        public void SetPool(EnemyPool p) => pool = p;

        /// <summary>스폰 시 EnemyData로 초기화하고 등장 상태로 진입.</summary>
        public void Spawn(EnemyData d, Vector3 position)
        {
            data = d;
            transform.position = position;
            hp = d.hp;
            state = EnemyState.Appear;
            appearTimer = 0f;
            fireTimer = d.fireInterval;
            facing = Vector2.down;
            spawnOrigin = position;
            pathIndex = 0;
            scattering = false;
            OnKilled = null;   // 풀 재사용 시 이전 구독 제거

            if (d.sprite != null) sr.sprite = d.sprite;
            SetAlpha(d.appearDuration > 0f ? 0f : 1f);

            var pgo = GameObject.FindWithTag("Player");
            player = pgo != null ? pgo.transform : null;

            gameObject.SetActive(true);
            if (CollisionManager.Instance != null) CollisionManager.Instance.RegisterTarget(this);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            switch (state)
            {
                case EnemyState.Appear:
                    appearTimer += dt;
                    float k = data.appearDuration > 0f ? Mathf.Clamp01(appearTimer / data.appearDuration) : 1f;
                    SetAlpha(k);
                    Move(dt);
                    if (k >= 1f) state = EnemyState.Act;
                    break;

                case EnemyState.Act:
                    Move(dt);
                    Fire(dt);
                    if (flashTimer > 0f) { flashTimer -= dt; sr.color = Color.white; }
                    else { Color c = data.color; c.a = 1f; sr.color = c; }
                    if (IsOffscreenBottom()) Despawn();   // 화면 아래로 빠지면 소멸(점수 없음)
                    break;
            }
        }

        /// <summary>편대 리더 격파 시 흩어짐: moveType 무시하고 지정 방향으로 이탈. (§5.8)</summary>
        public void Scatter(Vector2 dir)
        {
            if (state == EnemyState.Dead) return;
            scattering = true;
            scatterDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.down;
        }

        void Move(float dt)
        {
            Vector2 pos = transform.position;
            if (scattering)
            {
                pos += scatterDir * (data.moveSpeed * 1.7f * dt);
                transform.position = pos;
                return;
            }
            switch (data.moveType)
            {
                case EnemyMoveType.Homing:
                    if (player != null)
                    {
                        Vector2 want = ((Vector2)player.position - pos).normalized;
                        facing = data.homingTurnRate > 0f
                            ? Vector3.RotateTowards(facing, want, data.homingTurnRate * Mathf.Deg2Rad * dt, 0f)
                            : want;
                    }
                    pos += facing * (data.moveSpeed * dt);
                    break;

                case EnemyMoveType.PathTrajectory:
                    if (data.pathNodes != null && pathIndex < data.pathNodes.Length)
                    {
                        Vector2 target = spawnOrigin + data.pathNodes[pathIndex];
                        pos = Vector2.MoveTowards(pos, target, data.moveSpeed * dt);
                        if ((pos - target).sqrMagnitude < 0.01f) pathIndex++;
                    }
                    else pos += Vector2.down * (data.moveSpeed * dt);
                    break;

                default: // Straight / FixedCannon / 폴백: 아래로 직진
                    pos += Vector2.down * (data.moveSpeed * dt);
                    break;
            }
            transform.position = pos;
        }

        void Fire(float dt)
        {
            if (!data.canFire || BulletPool.Instance == null) return;
            fireTimer -= dt;
            if (fireTimer > 0f) return;
            fireTimer = data.fireInterval;

            Vector2 dir = player != null
                ? ((Vector2)player.position - (Vector2)transform.position).normalized
                : Vector2.down;

            var b = BulletPool.Instance.Get();
            b.Launch(transform.position, dir * data.bulletSpeed, data.bulletDamage,
                     data.bulletRadius, false, false, data.bulletColor, null);
        }

        /// <summary>피탄. 파괴 불가면 무시. 0 이하면 사망.</summary>
        public void TakeDamage(float amount)
        {
            if (state == EnemyState.Dead || !data.breakable) return;
            hp -= amount;
            flashTimer = 0.07f;
            if (AudioManager.Instance != null) AudioManager.Instance.Play("hit", 0.4f);
            if (hp <= 0f) Die();
        }

        void Die()
        {
            state = EnemyState.Dead;
            Vector3 pos = transform.position;
            if (GameManager.Instance != null) GameManager.Instance.AddKillScore(data.score);
            if (DropManager.Instance != null) DropManager.Instance.SpawnDrop(data, pos);
            if (EffectPool.Instance != null) EffectPool.Instance.Play(pos, 0.7f, new Color(1f, 0.7f, 0.3f, 1f));
            if (AudioManager.Instance != null) AudioManager.Instance.Play("explosion", 0.55f);
            OnKilled?.Invoke(this);
            Despawn();
        }

        void Despawn()
        {
            state = EnemyState.Dead;
            if (CollisionManager.Instance != null) CollisionManager.Instance.UnregisterTarget(this);
            if (pool != null) pool.Release(this);
            else gameObject.SetActive(false);
        }

        void SetAlpha(float a)
        {
            Color c = data != null ? data.color : sr.color;
            c.a = a;
            sr.color = c;
        }

        bool IsOffscreenBottom()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null || !cam.orthographic) return false;
            return transform.position.y < cam.transform.position.y - cam.orthographicSize - 1f;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}
