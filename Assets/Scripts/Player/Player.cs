using System;
using UnityEngine;
using ShootingGame.Core;

namespace ShootingGame.Player
{
    /// <summary>
    /// 기체의 중심 상태. 무기-출력(파워 레벨)과 라이프를 보유한다.
    /// 무기-출력은 기체의 속성이므로 무기를 교체해도 유지되고, 사망 시 리셋된다. (GDD §3)
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerMovement))]
    public class Player : MonoBehaviour
    {
        [SerializeField] PlayerData data;

        public PlayerData Data => data;
        public int Lives { get; private set; }
        public int PowerLevel { get; private set; }
        public int Bombs { get; private set; }
        public bool IsInvulnerable => invulnTimer > 0f;
        public float HitboxRadius => data != null ? data.hitboxRadius : 0.12f;

        float invulnTimer;
        Vector3 spawnPosition;
        PlayerInputReader input;

        public event Action<int> LivesChanged;       // 잔기 변경
        public event Action<int> PowerLevelChanged;   // 무기-출력 변경
        public event Action<int> BombsChanged;        // 봄 보유 변경
        public event Action Died;                     // 한 기 사망
        public event Action GameOver;                 // 잔기 소진

        void Awake()
        {
            spawnPosition = transform.position;
            input = GetComponent<PlayerInputReader>();
            Lives = data.startLives;
            Bombs = data.startBombs;
            PowerLevel = ClampPower(data.startPowerLevel);
        }

        void Start()
        {
            LivesChanged?.Invoke(Lives);
            PowerLevelChanged?.Invoke(PowerLevel);
            BombsChanged?.Invoke(Bombs);
        }

        void Update()
        {
            if (invulnTimer > 0f)
                invulnTimer -= Time.deltaTime;

            if (input != null && input.BombPressed
                && (GameManager.Instance == null || GameManager.Instance.IsPlaying))
                UseBomb();
        }

        /// <summary>잔기 추가(1UP). (GDD §7 익스텐드)</summary>
        public void AddLife(int amount = 1)
        {
            if (amount <= 0) return;
            Lives += amount;
            LivesChanged?.Invoke(Lives);
        }

        /// <summary>봄(특수공격): 화면 전체 클리어 + 무적. (GDD §4)</summary>
        public void UseBomb()
        {
            if (Bombs <= 0) return;
            Bombs--;
            BombsChanged?.Invoke(Bombs);
            invulnTimer = Mathf.Max(invulnTimer, data.bombInvulnSeconds);
            if (CollisionManager.Instance != null)
                CollisionManager.Instance.BlastRadius(transform.position, data.bombRadius, data.bombDamage);
            if (AudioManager.Instance != null) AudioManager.Instance.Play("bomb", 0.9f);
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.6f, 0.5f);
            if (GameManager.Instance != null) GameManager.Instance.HitStop(0.06f);
            if (EffectPool.Instance != null) EffectPool.Instance.Play(transform.position, 4f, new Color(0.7f, 0.95f, 1f, 1f));
        }

        /// <summary>파워업(P) 아이템 1개당 +1단계. (GDD §3)</summary>
        public void AddPower(int amount = 1)
        {
            int next = ClampPower(PowerLevel + amount);
            if (next == PowerLevel) return;
            PowerLevel = next;
            PowerLevelChanged?.Invoke(PowerLevel);
        }

        /// <summary>
        /// 피탄 처리. 무적 중이면 무시(false). 1기 소모 후 무기-출력을 리셋한다.
        /// 외부 충돌 시스템(원-원 판정)이 호출한다.
        /// </summary>
        public bool TryHit()
        {
            if (IsInvulnerable) return false;

            if (AudioManager.Instance != null) AudioManager.Instance.Play("death", 0.7f);
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.4f, 0.4f);
            if (GameManager.Instance != null) GameManager.Instance.HitStop(0.08f);
            if (EffectPool.Instance != null) EffectPool.Instance.Play(transform.position, 1.2f, new Color(0.6f, 0.9f, 1f, 1f));

            Lives--;
            LivesChanged?.Invoke(Lives);

            // 사망 시 무기-출력 리셋 (GDD §3)
            PowerLevel = ClampPower(data.startPowerLevel);
            PowerLevelChanged?.Invoke(PowerLevel);

            Died?.Invoke();

            if (Lives <= 0)
            {
                GameOver?.Invoke();
                gameObject.SetActive(false);
            }
            else
            {
                Respawn();
            }
            return true;
        }

        void Respawn()
        {
            transform.position = spawnPosition;
            invulnTimer = data.invulnerabilitySeconds;
        }

        int ClampPower(int level) => Mathf.Clamp(level, 1, data.maxPowerLevel);

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, HitboxRadius);
        }
    }
}
