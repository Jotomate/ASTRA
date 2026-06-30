using UnityEngine;

namespace ShootingGame.Enemy
{
    /// <summary>이동 방식 (GDD §5.1, §5.6). FixedCannon=지형 고정 대포(아래로 스크롤).</summary>
    public enum EnemyMoveType { Straight, PathTrajectory, Formation, Homing, FixedCannon }

    /// <summary>처치 시 드롭 종류 (GDD §5.1).</summary>
    public enum DropType { None, PowerUp, Weapon }

    /// <summary>
    /// 개체 적 1종 = 카드 1장. Enemy 런타임이 이 카드를 읽어 작동한다.
    /// 새 적 추가 = 카드 추가. (GDD §5.1, CLAUDE.md §3.1)
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "ASTRA/Enemy Data", order = 2)]
    public class EnemyData : ScriptableObject
    {
        [Header("식별 / 공통")]
        public string enemyName = "Grunt";
        [Min(1)] public float hp = 3f;
        public int score = 100;
        [Tooltip("파괴 가능 여부 (불가면 탄에 안 죽음)")]
        public bool breakable = true;

        [Header("외형 / 판정")]
        public Sprite sprite;
        public Color color = new Color(1f, 0.35f, 0.35f, 1f);
        [Tooltip("피탄 판정 반지름(월드 유닛)")]
        public float hitboxRadius = 0.3f;

        [Header("등장 (허공 등장 알파 페이드, §5.4)")]
        [Tooltip("페이드인 시간(초). 0이면 즉시 등장")]
        public float appearDuration = 0.3f;

        [Header("이동")]
        public EnemyMoveType moveType = EnemyMoveType.Straight;
        [Tooltip("이동 속도(월드 유닛/초)")]
        public float moveSpeed = 3f;
        [Tooltip("Homing: 초당 선회 각도 제한(도). 0이면 즉시 조준")]
        public float homingTurnRate = 120f;
        [Tooltip("PathTrajectory: 스폰 위치 기준 상대 웨이포인트(순서대로 통과)")]
        public Vector2[] pathNodes;

        [Header("발사 (조준탄)")]
        public bool canFire = false;
        [Tooltip("발사 간격(초)")]
        public float fireInterval = 1.5f;
        public float bulletSpeed = 6f;
        public float bulletRadius = 0.12f;
        public float bulletDamage = 1f;
        public Color bulletColor = new Color(1f, 0.5f, 0.3f, 1f);

        [Header("드롭 (처치 시)")]
        public DropType dropType = DropType.None;
        [Range(0f, 1f)] public float dropChance = 0.3f;
        [Tooltip("Weapon 드롭일 때 부여할 무기 카드")]
        public ShootingGame.Weapon.WeaponData weaponDrop;
    }
}
