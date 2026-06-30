using UnityEngine;

namespace ShootingGame.Player
{
    /// <summary>
    /// 기체(Player)의 튜닝 수치를 담는 데이터 카드.
    /// 무기-출력(파워 레벨)은 기체의 속성이며, 교체해도 유지·사망 시 리셋된다. (GDD §3, §2)
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "ASTRA/Player Data", order = 0)]
    public class PlayerData : ScriptableObject
    {
        [Header("이동")]
        [Tooltip("이동 속도 (월드 유닛/초)")]
        public float moveSpeed = 8f;
        [Tooltip("플레이 영역 가장자리에서 남길 여백 (월드 유닛)")]
        public float screenPadding = 0.3f;

        [Header("무기-출력 (기체 속성, 최대 4단계)")]
        [Min(1)] public int maxPowerLevel = 4;
        [Min(1)] public int startPowerLevel = 1;

        [Header("라이프")]
        [Min(1)] public int startLives = 3;
        [Tooltip("피격(부활) 후 무적 시간 (초)")]
        public float invulnerabilitySeconds = 1.5f;

        [Header("충돌 (원-원 거리 비교)")]
        [Tooltip("피탄 판정 반지름 (월드 유닛). 종스크롤 슈팅은 작게.")]
        public float hitboxRadius = 0.12f;

        [Header("특수공격 (봄 — 화면 클리어)")]
        [Min(0)] public int startBombs = 2;
        [Tooltip("봄 폭발 반경(월드 유닛, 화면 전체 커버)")]
        public float bombRadius = 30f;
        public float bombDamage = 9999f;
        [Tooltip("봄 사용 후 무적 시간(초)")]
        public float bombInvulnSeconds = 1.5f;
    }
}
