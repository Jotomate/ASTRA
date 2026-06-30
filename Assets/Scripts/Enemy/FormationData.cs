using UnityEngine;

namespace ShootingGame.Enemy
{
    /// <summary>편대 배열 형태.</summary>
    public enum FormationPattern { Line, VShape, Column }

    /// <summary>
    /// 스폰 시 등장하는 적 무리(편대). 동일 EnemyData를 패턴대로 배열해 스폰한다. (GDD §5.1)
    /// </summary>
    [CreateAssetMenu(fileName = "FormationData", menuName = "ASTRA/Formation Data", order = 5)]
    public class FormationData : ScriptableObject
    {
        public string formationName = "Wave";
        [Tooltip("편대 구성원 적 카드")]
        public EnemyData enemyData;
        [Min(1)] public int count = 5;
        public FormationPattern pattern = FormationPattern.Line;
        [Tooltip("구성원 간 간격(월드 유닛)")]
        public float spacing = 1.3f;

        [Header("튜닝 placeholder (§5.8)")]
        [Tooltip("리더 격파 시 나머지 흩어짐 — 미구현")]
        public bool leaderBreakScatter = true;
        [Tooltip("편대 전멸 시 보너스 — 미구현")]
        public bool wipeBonus = true;
    }
}
