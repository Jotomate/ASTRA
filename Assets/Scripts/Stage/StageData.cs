using UnityEngine;

namespace ShootingGame.Stage
{
    /// <summary>배경 스크롤 레이어 1장(다중 스크롤/시차, 책 §7.4).</summary>
    [System.Serializable]
    public class BackgroundLayer
    {
        public string name = "Layer";
        public Sprite sprite;
        [Tooltip("스크롤 속도 배율 (baseScrollSpeed에 곱). 깊은 층=작게, 얕은 층=크게")]
        public float scrollFactor = 1f;
        [Tooltip("정렬 순서(배경이므로 음수 권장)")]
        public int sortingOrder = -100;
        public Color tint = Color.white;
    }

    /// <summary>스폰 이벤트 종류.</summary>
    public enum SpawnKind { Enemy, Formation, BossWarning, Boss, Terrain }

    /// <summary>스폰 타임라인의 1개 이벤트 (거리/시간 트리거 → 적/편대/보스). (§6.1)</summary>
    [System.Serializable]
    public class SpawnEvent
    {
        public string label = "Event";
        [Tooltip("스테이지 시작 후 발동 시간(초)")]
        public float time = 1f;
        public SpawnKind kind = SpawnKind.Enemy;
        public ShootingGame.Enemy.EnemyData enemy;
        public ShootingGame.Enemy.FormationData formation;
        public ShootingGame.Boss.BossData boss;
        [Tooltip("스폰 X 위치(상단). Enemy/Formation 기준 중심")]
        public float spawnX = 0f;
        [Tooltip("Enemy: 추가 X 랜덤 범위(±)")]
        public float spawnXJitter = 0f;
    }

    /// <summary>
    /// 스테이지 1종 = 카드 1장. 배경(다중 스크롤) + 스폰 타임라인(구간·지형은 추후 확장). (§6.1)
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "ASTRA/Stage Data", order = 3)]
    public class StageData : ScriptableObject
    {
        [Header("배경 (다중 스크롤)")]
        [Tooltip("기본 스크롤 속도(월드 유닛/초). 각 레이어 scrollFactor에 곱해진다")]
        public float baseScrollSpeed = 2f;
        public BackgroundLayer[] layers;

        [Header("스폰 타임라인")]
        [Tooltip("시간 순서대로 발동되는 스폰 이벤트들")]
        public SpawnEvent[] spawnEvents;
    }
}
