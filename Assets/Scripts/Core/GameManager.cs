using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using PlayerShip = ShootingGame.Player.Player;

namespace ShootingGame.Core
{
    public enum GameState { Title, Playing, Paused, GameOver }

    /// <summary>
    /// 게임 전역 상태: 점수 + 플레이/게임오버 흐름. 추후 난이도·플로우 등 §7 확장 지점.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int Score { get; private set; }
        public int Stage { get; private set; } = 1;
        public GameState State { get; private set; } = GameState.Title;
        public bool IsPlaying => State == GameState.Playing;

        [Tooltip("이 점수마다 1UP(익스텐드)")]
        [SerializeField] int extendScoreInterval = 30000;
        int nextExtend;

        public event Action<int> ScoreChanged;
        public event Action<GameState> StateChanged;

        // 보스 HUD 연동(보스 타입에 의존하지 않도록 이벤트로 통지)
        public event Action<string> BossSpawned;
        public event Action<float> BossHpChanged;
        public event Action BossDefeated;

        // 스테이지 흐름
        public event Action BossWarning;
        public event Action StageClear;
        public event Action<int> StageChanged;

        PlayerShip player;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            nextExtend = extendScoreInterval;
        }

        void OnDestroy()
        {
            if (player != null) player.GameOver -= OnPlayerGameOver;
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            EnsurePlayer();
            SetState(GameState.Title);   // 타이틀에서 시작(게임 정지)
        }

        void SetState(GameState s)
        {
            State = s;
            Time.timeScale = s == GameState.Playing ? 1f : 0f;
            StateChanged?.Invoke(s);
        }

        void EnsurePlayer()
        {
            if (player != null) return;   // Unity 가짜-null 인식
            var pgo = GameObject.FindWithTag("Player");
            if (pgo == null) return;
            player = pgo.GetComponent<PlayerShip>();
            if (player != null) player.GameOver += OnPlayerGameOver;
        }

        void Update()
        {
            switch (State)
            {
                case GameState.Title:
                    if (StartPressed()) SetState(GameState.Playing);
                    break;
                case GameState.Playing:
                    if (PausePressed()) SetState(GameState.Paused);
                    break;
                case GameState.Paused:
                    if (PausePressed()) SetState(GameState.Playing);
                    break;
                case GameState.GameOver:
                    if (RestartPressed()) Restart();
                    break;
            }
        }

        public void AddScore(int amount)
        {
            if (amount == 0) return;
            Score += amount;
            ScoreChanged?.Invoke(Score);

            // 1UP(익스텐드)
            if (Score >= nextExtend) EnsurePlayer();
            while (extendScoreInterval > 0 && Score >= nextExtend)
            {
                if (player != null) player.AddLife(1);
                nextExtend += extendScoreInterval;
            }
        }

        /// <summary>다음 스테이지로(멀티 스테이지 진행).</summary>
        public void NextStage()
        {
            Stage++;
            StageChanged?.Invoke(Stage);
        }

        public void NotifyBossSpawned(string name) => BossSpawned?.Invoke(name);
        public void NotifyBossHp(float ratio) => BossHpChanged?.Invoke(ratio);
        public void NotifyBossDefeated() => BossDefeated?.Invoke();
        public void NotifyBossWarning()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play("warn", 0.8f);
            BossWarning?.Invoke();
        }
        public void NotifyStageClear() => StageClear?.Invoke();

        void OnPlayerGameOver()
        {
            if (State == GameState.GameOver) return;
            SetState(GameState.GameOver);   // 화면 정지
        }

        void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        static bool RestartPressed()
        {
            bool key = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            bool pad = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
            return key || pad;
        }

        static bool StartPressed()
        {
            var k = Keyboard.current;
            bool key = k != null && (k.zKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
            var g = Gamepad.current;
            bool pad = g != null && (g.buttonSouth.wasPressedThisFrame || g.startButton.wasPressedThisFrame);
            return key || pad;
        }

        static bool PausePressed()
        {
            var k = Keyboard.current;
            bool key = k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame);
            var g = Gamepad.current;
            bool pad = g != null && g.startButton.wasPressedThisFrame;
            return key || pad;
        }
    }
}
