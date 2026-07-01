using System.Collections.Generic;
using UnityEngine;
using ShootingGame.Player;
using ShootingGame.Bullet;
using ShootingGame.Core;

namespace ShootingGame.Weapon
{
    /// <summary>
    /// 장착 무기(WeaponData)를 해석해 발사한다. 무기-출력(파워 레벨)은 기체(Player)에서 읽는다.
    /// 교체 모듈형: Equip()으로 즉시 교체. 기본 무기(Vulcan)는 항상 보유. (GDD §3)
    /// </summary>
    [RequireComponent(typeof(Player.Player))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] WeaponData defaultWeapon;   // 기본 무기 Vulcan (항상 보유)
        [SerializeField] Transform muzzle;           // 비우면 기체 위치에서 발사

        [Header("이탈(Eject) — 장착 무기 해제 + 반경 폭발")]
        [Tooltip("폭발 반경(월드 유닛). 이 안의 적/적탄을 휩쓴다.")]
        [SerializeField] float ejectRadius = 2.2f;
        [Tooltip("폭발 반경 내 적에 가하는 데미지")]
        [SerializeField] float ejectDamage = 50f;
        [SerializeField] ShootingGame.Effect.ExplosionEffect explosion;

        [Header("연출")]
        [Tooltip("발사 시 총구 섬광(비우면 생략)")]
        [SerializeField] ShootingGame.Effect.MuzzleFlash muzzleFlash;

        public WeaponData Current { get; private set; }
        /// <summary>기본 무기가 아닌 무기를 장착 중인가(이탈 가능 상태).</summary>
        public bool HasEquippedWeapon => Current != null && Current != defaultWeapon;
        /// <summary>장착 무기 변경 시 발생(HUD 등 구독용).</summary>
        public event System.Action<WeaponData> WeaponChanged;

        /// <summary>차지 무기 홀드 충전량 0~1 (차지 무기 아니거나 미충전 시 0). HUD 게이지용.</summary>
        public float ChargeLevel =>
            Current != null && Current.fireMode == FireMode.Charge && Current.maxChargeTime > 0f
                ? Mathf.Clamp01(chargeTime / Current.maxChargeTime) : 0f;

        Player.Player player;
        PlayerInputReader input;
        float cooldown;
        float spinAngle;    // 전방위샷 회전 누적각
        float chargeTime;   // 차지 홀드 누적
        float pendingCharge;// 발사 직전 소비할 차지 레벨
        bool wasFiring, wasFull;

        void Awake()
        {
            player = GetComponent<Player.Player>();
            input = GetComponent<PlayerInputReader>();
            Current = defaultWeapon;
        }

        void Start()
        {
            WeaponChanged?.Invoke(Current);
        }

        void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            if (cooldown > 0f) cooldown -= Time.deltaTime;

            if (input.EjectPressed)
                Eject();

            if (Current == null) { wasFiring = false; return; }

            bool firing = input.IsFiring;

            if (Current.fireMode == FireMode.Charge)
            {
                if (firing)
                {
                    chargeTime += Time.deltaTime;               // 홀드 → 충전
                    if (!wasFull && ChargeLevel >= 1f)          // 완충 알림음(1회)
                    {
                        wasFull = true;
                        if (AudioManager.Instance != null) AudioManager.Instance.Play("power", 0.5f);
                    }
                }
                else if (wasFiring)                             // 뗀 순간 → 발사
                {
                    pendingCharge = ChargeLevel;
                    Fire();
                    chargeTime = 0f; wasFull = false;
                }
            }
            else if (firing && cooldown <= 0f)                  // 일반: 유지 연사
            {
                Fire();
                cooldown = Current.GetFireInterval(player.PowerLevel);
            }

            wasFiring = firing;
        }

        /// <summary>
        /// 이탈: 장착 무기를 주변 반경 폭발과 함께 해제하고 기본 무기로 복귀한다.
        /// 기본 무기 상태면 아무 일도 일어나지 않는다.
        /// </summary>
        public void Eject()
        {
            if (!HasEquippedWeapon) return;

            Vector2 center = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
            if (explosion != null)
                explosion.Play(center, ejectRadius);

            ApplyEjectBlast(center, ejectRadius);
            Equip(defaultWeapon);   // 기본 상태로 복귀
        }

        /// <summary>반경 내 적에 데미지 + 적탄 소거. (GDD §5)</summary>
        void ApplyEjectBlast(Vector2 center, float radius)
        {
            var cm = ShootingGame.Core.CollisionManager.Instance;
            if (cm != null) cm.BlastRadius(center, radius, ejectDamage);
        }

        /// <summary>필드에서 무기를 주우면 즉시 교체. null이면 기본 무기로. (GDD §3 — 즉시 교체형)</summary>
        public void Equip(WeaponData weapon)
        {
            Current = weapon != null ? weapon : defaultWeapon;
            cooldown = 0f;
            spinAngle = 0f;   // 무기 교체 시 전방위 회전각 초기화 (다음 무기 발사각 틀어짐 방지)
            chargeTime = 0f; wasFull = false; pendingCharge = 0f;   // 차지 리셋
            WeaponChanged?.Invoke(Current);
        }

        /// <summary>현재 무기-출력에 맞춰 1회 발사. 외부(테스트/특수 트리거)에서도 호출 가능.</summary>
        public void Fire()
        {
            BulletPool poolInst = BulletPool.Instance;
            if (poolInst == null || Current == null) return;

            // 차지 배율 (일반 발사는 t=0 → 배율 1)
            float t = pendingCharge; pendingCharge = 0f;
            bool charged = t > 0.05f;
            float dmgMul = Mathf.Lerp(1f, Current.chargeDamageMul, t);
            float radius = Current.bulletRadius * Mathf.Lerp(1f, Current.chargeSizeMul, t);
            bool pierce = Current.isPiercing || (t >= 0.9f && Current.chargePiercing);

            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(Current.isPiercing ? "laser" : "shoot", charged ? 0.6f : 0.28f);

            int level = player.PowerLevel;
            int ways = Mathf.Max(1, Current.GetWayCount(level));
            float dmg = Current.GetDamage(level) * dmgMul;
            Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;

            if (muzzleFlash != null)
                muzzleFlash.Flash(origin, Current.bulletColor, charged ? 1.7f : 1f);

            if (Current.isLockOn) { FireLockOn(poolInst, origin, ways, dmg, radius, pierce); return; }

            float baseAngle = 90f + spinAngle;   // 위쪽 + 전방위 회전
            float half = Current.spreadAngle * 0.5f;
            for (int i = 0; i < ways; i++)
            {
                float angle = baseAngle;
                if (ways > 1)
                    angle = baseAngle - half + (Current.spreadAngle * i / (ways - 1));

                float rad = angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                ShootingGame.Bullet.Bullet b = poolInst.Get();
                b.Launch(origin, dir * Current.shotSpeed, dmg, radius,
                         true, pierce, Current.bulletColor, Current.bulletSprite,
                         Current.isReflecting, Current.maxBounces,
                         Current.isHoming, Current.homingTurnRate);
            }

            if (Current.spinRate != 0f)
                spinAngle = Mathf.Repeat(spinAngle + Current.spinRate, 360f);
        }

        readonly List<Transform> lockBuf = new List<Transform>(8);

        /// <summary>록온: 가까운 적 다수를 잡아 각각에 유도 레이저 발사. 대상 없으면 부채꼴 유도탄.</summary>
        void FireLockOn(BulletPool pool, Vector2 origin, int count, float dmg, float radius, bool pierce)
        {
            if (CollisionManager.Instance != null) CollisionManager.Instance.FindNearestTargets(origin, count, lockBuf);
            else lockBuf.Clear();

            for (int i = 0; i < count; i++)
            {
                Transform target = lockBuf.Count > 0 ? lockBuf[i % lockBuf.Count] : null;
                Vector2 dir = target != null
                    ? ((Vector2)target.position - origin).normalized
                    : AngleDir(count > 1 ? 90f - 20f + 40f * i / (count - 1) : 90f);
                var b = pool.Get();
                b.Launch(origin, dir * Current.shotSpeed, dmg, radius, true, pierce,
                         Current.bulletColor, Current.bulletSprite, Current.isReflecting, Current.maxBounces,
                         true, Current.homingTurnRate, target);
            }
        }

        static Vector2 AngleDir(float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
        }
    }
}
