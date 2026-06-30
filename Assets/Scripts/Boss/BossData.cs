using UnityEngine;

namespace ShootingGame.Boss
{
    /// <summary>보스 탄막 패턴 종류 (페이즈별 선택).</summary>
    public enum BossPatternType { Aimed, NWay, Circle }

    /// <summary>파괴 가능한 보스 부위(포탑·약점). 보스 중심 기준 localPos에 배치된다. (§5.10)</summary>
    [System.Serializable]
    public class BossPartDef
    {
        public string name = "Cannon";
        public Sprite sprite;
        public Color color = Color.white;
        public float scale = 1f;
        public Vector2 localPos;
        public float hp = 50f;
        public float hitRadius = 0.4f;
        public int score = 500;

        [Header("부위 발사(조준탄)")]
        public bool canFire = true;
        public float fireInterval = 1.6f;
        public float bulletSpeed = 5f;
        public float bulletDamage = 1f;
        public float bulletRadius = 0.13f;
        public Color bulletColor = new Color(1f, 0.6f, 0.3f, 1f);
    }

    /// <summary>체력 구간별 페이즈(이동 + 탄막). hpThreshold 내림차순으로 정의. (§5.11)</summary>
    [System.Serializable]
    public class BossPhaseDef
    {
        public string name = "Phase";
        [Tooltip("코어 체력 비율이 이 값 이하로 떨어지면 이 페이즈로 전환")]
        [Range(0f, 1f)] public float hpThreshold = 1f;

        [Header("이동(좌우 사인 왕복)")]
        public float moveAmplitudeX = 2.5f;
        public float moveSpeed = 1.2f;

        [Header("탄막")]
        public BossPatternType pattern = BossPatternType.Aimed;
        public float fireInterval = 1.2f;
        [Tooltip("n-way/원형 탄 수")]
        public int wayCount = 5;
        public float spreadAngle = 60f;
        public float bulletSpeed = 5f;
        public float bulletDamage = 1f;
        public float bulletRadius = 0.14f;
        public Color bulletColor = new Color(1f, 0.4f, 0.6f, 1f);
    }

    /// <summary>
    /// 보스 1종 = 카드 1장. 파츠 + 페이즈 + 코어. "부품의 집합"으로 설계. (GDD §5.2, CLAUDE.md §7)
    /// </summary>
    [CreateAssetMenu(fileName = "BossData", menuName = "ASTRA/Boss Data", order = 4)]
    public class BossData : ScriptableObject
    {
        [Header("식별 / 본체")]
        public string bossName = "Battleship";
        public Sprite bodySprite;
        public Color bodyColor = Color.white;
        public float bodyScale = 2.5f;

        [Header("코어")]
        public float coreHp = 300f;
        [Tooltip("코어(약점) 피탄 반지름. 본체보다 작게 = 중앙을 노려야 함")]
        public float coreHitRadius = 0.7f;
        public int score = 10000;

        [Header("등장")]
        public float entrySpeed = 2f;
        [Tooltip("자리잡는 Y 위치")]
        public float battleY = 2.5f;

        [Header("기믹 — 변신 (§5.13)")]
        public bool useTransform = false;
        [Tooltip("코어 체력 비율이 이 값 이하가 되면 변신")]
        [Range(0f, 1f)] public float transformAtHp = 0.5f;
        public Sprite transformSprite;
        public Color transformColor = Color.white;
        [Tooltip("변신 중 무적/점멸 시간(초)")]
        public float transformInvulnTime = 1.2f;

        [Header("기믹 — 분리 (사망 시, §5.12)")]
        public bool separateOnDeath = false;
        [Tooltip("분리되어 흩어지는 잔해 적")]
        public ShootingGame.Enemy.EnemyData separatedEnemy;
        public int separatedCount = 4;

        [Header("기믹 — 촉수 (§5.14)")]
        public bool useTentacles = false;
        public Sprite tentacleSprite;
        public int tentacleCount = 2;
        public int tentacleSegments = 6;
        public float tentacleReach = 4f;
        public float tentacleSpeed = 4f;

        public BossPartDef[] parts;
        [Tooltip("hpThreshold 내림차순(1.0 → 낮게)으로 정의")]
        public BossPhaseDef[] phases;
    }
}
