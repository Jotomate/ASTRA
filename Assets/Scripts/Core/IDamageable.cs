using UnityEngine;

namespace ShootingGame.Core
{
    /// <summary>
    /// 자기탄/폭발에 피격될 수 있는 대상(적 개체, 보스 파츠 등). 원-원 충돌의 공통 인터페이스.
    /// </summary>
    public interface IDamageable
    {
        Transform Transform { get; }
        float HitRadius { get; }
        bool IsDead { get; }
        void TakeDamage(float amount);
    }
}
