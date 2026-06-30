using UnityEngine;

namespace ShootingGame.Weapon
{
    public enum FireMode { Single, Rapid, Charge, SemiAuto }

    /// <summary>
    /// 무기 1종 = 카드 1장. 발사 로직(WeaponController)이 이 카드를 읽어 작동한다.
    /// 새 무기 추가 = 카드 추가(기존 동작 재사용 시 코드 무수정). (GDD §3, §3.2)
    /// ※ 무기-출력(파워 레벨)은 기체 속성이며 여기 저장하지 않는다 — 강화 '곡선'만 정의.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "ASTRA/Weapon Data", order = 1)]
    public class WeaponData : ScriptableObject
    {
        [Header("식별")]
        public string weaponName = "Vulcan";
        public Sprite icon;   // 전환 UI용

        [Header("발사 공통 특성")]
        public FireMode fireMode = FireMode.Rapid;
        [Tooltip("탄 속도 (월드 유닛/초)")]
        public float shotSpeed = 14f;
        [Tooltip("기본 위력")]
        public float baseDamage = 1f;
        [Tooltip("거리 감쇠 (0 = 감쇠 없음) — 미구현 placeholder, §4.11")]
        public float damageDecay = 0f;
        [Tooltip("관통 여부 — 무기 고유 특성(레벨 비례 아님)")]
        public bool isPiercing = false;
        [Tooltip("n-way 전체 펼침 각도(도). 1-way면 무시")]
        public float spreadAngle = 14f;

        [Header("탄 외형 / 판정")]
        public Sprite bulletSprite;                                   // 비우면 프리팹 기본 스프라이트
        public Color bulletColor = new Color(0.7f, 0.95f, 1f, 1f);
        [Tooltip("탄 충돌 반지름(월드 유닛) = hitboxSize")]
        public float bulletRadius = 0.1f;

        [Header("반사 (지형 의존 무기 — 벽 반사)")]
        public bool isReflecting = false;
        public int maxBounces = 0;

        [Header("유도 / 전방위")]
        [Tooltip("락샷: 탄이 최근접 적을 추격")]
        public bool isHoming = false;
        [Tooltip("유도 선회 속도(도/초)")]
        public float homingTurnRate = 240f;
        [Tooltip("전방위샷: 발사 기준각이 매 발사마다 회전(도). 0이면 정면 고정")]
        public float spinRate = 0f;
        [Tooltip("록온: 주변 적 다수를 잡아 각각에 유도탄 발사(wayCount=동시 락온 수)")]
        public bool isLockOn = false;

        [Header("무기-출력 강화 곡선 (Lv.1~4)")]
        [Tooltip("동시 발사 수")]
        public int[] wayCount = new int[4] { 1, 2, 3, 4 };
        [Tooltip("위력 배율 (baseDamage에 곱)")]
        public float[] damageMul = new float[4] { 1f, 1.25f, 1.6f, 2f };
        [Tooltip("발사 간격(초) — 작을수록 빠름")]
        public float[] fireInterval = new float[4] { 0.16f, 0.13f, 0.11f, 0.09f };

        public int GetWayCount(int level) => SampleInt(wayCount, level, 1);
        public float GetDamage(int level) => baseDamage * SampleFloat(damageMul, level, 1f);
        public float GetFireInterval(int level) => SampleFloat(fireInterval, level, 0.15f);

        static int SampleInt(int[] a, int level, int fallback)
        {
            if (a == null || a.Length == 0) return fallback;
            return a[Mathf.Clamp(level - 1, 0, a.Length - 1)];
        }

        static float SampleFloat(float[] a, int level, float fallback)
        {
            if (a == null || a.Length == 0) return fallback;
            return a[Mathf.Clamp(level - 1, 0, a.Length - 1)];
        }
    }
}
