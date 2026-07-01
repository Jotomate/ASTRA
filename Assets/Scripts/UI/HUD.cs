using UnityEngine;
using UnityEngine.UI;
using ShootingGame.Core;
using ShootingGame.Weapon;
using PlayerShip = ShootingGame.Player.Player;

namespace ShootingGame.UI
{
    /// <summary>
    /// 라이프 · 점수 · 무기-출력 · 현재 무기를 표시하는 HUD.
    /// Player / GameManager / WeaponController의 이벤트를 구독해 갱신한다.
    /// (TMP 에센셜 임포트 후 TextMeshProUGUI로 교체 가능)
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] Text livesText;
        [SerializeField] Text scoreText;
        [SerializeField] Text comboText;
        [SerializeField] Text powerText;
        [SerializeField] Text weaponText;
        [SerializeField] Text bombText;
        [SerializeField] Text gameOverText;

        [Header("화면")]
        [SerializeField] GameObject titleRoot;
        [SerializeField] Text pausedText;

        [Header("차지 게이지")]
        [SerializeField] GameObject chargeBarRoot;
        [SerializeField] Image chargeFill;

        [Header("보스")]
        [SerializeField] GameObject bossBar;
        [SerializeField] Image bossFill;
        [SerializeField] Text bossNameText;

        [Header("스테이지")]
        [SerializeField] Text warningText;
        [SerializeField] Text stageClearText;
        [SerializeField] Text stageNumberText;

        bool warningActive;

        PlayerShip player;
        WeaponController weapon;

        void Start()
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo != null)
            {
                player = pgo.GetComponent<PlayerShip>();
                weapon = pgo.GetComponent<WeaponController>();
            }

            if (player != null)
            {
                player.LivesChanged += OnLives;
                player.PowerLevelChanged += OnPower;
                player.BombsChanged += OnBombs;
                OnLives(player.Lives);
                OnPower(player.PowerLevel);
                OnBombs(player.Bombs);
            }
            if (weapon != null)
            {
                weapon.WeaponChanged += OnWeapon;
                OnWeapon(weapon.Current);
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ScoreChanged += OnScore;
                GameManager.Instance.StateChanged += OnState;
                GameManager.Instance.BossSpawned += OnBossSpawned;
                GameManager.Instance.BossHpChanged += OnBossHp;
                GameManager.Instance.BossDefeated += OnBossDefeated;
                GameManager.Instance.BossWarning += OnBossWarning;
                GameManager.Instance.StageClear += OnStageClear;
                GameManager.Instance.StageChanged += OnStageChanged;
                GameManager.Instance.ComboChanged += OnCombo;
                OnScore(GameManager.Instance.Score);
                OnState(GameManager.Instance.State);
                OnStageChanged(GameManager.Instance.Stage);
                OnCombo(0, 1);
            }
            if (bossBar != null) bossBar.SetActive(false);
            if (warningText != null) warningText.gameObject.SetActive(false);
            if (stageClearText != null) stageClearText.gameObject.SetActive(false);
        }

        void Update()
        {
            if (warningActive && warningText != null)
                warningText.enabled = Mathf.FloorToInt(Time.unscaledTime * 4f) % 2 == 0;

            if (weapon != null && chargeFill != null)
            {
                float c = weapon.ChargeLevel;
                if (chargeBarRoot != null) chargeBarRoot.SetActive(c > 0.01f);
                chargeFill.fillAmount = c;
                chargeFill.color = c >= 1f ? new Color(1f, 0.95f, 0.5f) : new Color(0.5f, 0.8f, 1f);
            }
        }

        void OnDestroy()
        {
            if (player != null)
            {
                player.LivesChanged -= OnLives;
                player.PowerLevelChanged -= OnPower;
                player.BombsChanged -= OnBombs;
            }
            if (weapon != null) weapon.WeaponChanged -= OnWeapon;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ScoreChanged -= OnScore;
                GameManager.Instance.StateChanged -= OnState;
                GameManager.Instance.BossSpawned -= OnBossSpawned;
                GameManager.Instance.BossHpChanged -= OnBossHp;
                GameManager.Instance.BossDefeated -= OnBossDefeated;
                GameManager.Instance.BossWarning -= OnBossWarning;
                GameManager.Instance.StageClear -= OnStageClear;
                GameManager.Instance.StageChanged -= OnStageChanged;
                GameManager.Instance.ComboChanged -= OnCombo;
            }
        }

        void OnBombs(int v) { if (bombText != null) bombText.text = "BOMB  x" + v; }

        void OnCombo(int combo, int mult)
        {
            if (comboText == null) return;
            if (combo >= 2) { comboText.text = combo + " COMBO  x" + mult; comboText.enabled = true; }
            else comboText.enabled = false;
        }

        void OnStageChanged(int n)
        {
            if (stageNumberText != null) stageNumberText.text = "STAGE " + n;
            if (stageClearText != null) stageClearText.gameObject.SetActive(false);
        }

        void OnBossWarning()
        {
            warningActive = true;
            if (warningText != null) warningText.gameObject.SetActive(true);
        }

        void OnStageClear()
        {
            if (stageClearText != null) stageClearText.gameObject.SetActive(true);
        }

        void OnState(GameState s)
        {
            if (titleRoot != null) titleRoot.SetActive(s == GameState.Title);
            if (pausedText != null) pausedText.gameObject.SetActive(s == GameState.Paused);
            if (gameOverText != null) gameOverText.gameObject.SetActive(s == GameState.GameOver);
        }

        void OnBossSpawned(string name)
        {
            warningActive = false;
            if (warningText != null) warningText.gameObject.SetActive(false);
            if (bossNameText != null) bossNameText.text = name;
            if (bossFill != null) bossFill.fillAmount = 1f;
            if (bossBar != null) bossBar.SetActive(true);
        }

        void OnBossHp(float ratio)
        {
            if (bossFill != null) bossFill.fillAmount = ratio;
        }

        void OnBossDefeated()
        {
            if (bossBar != null) bossBar.SetActive(false);
        }

        void OnLives(int v)  { if (livesText  != null) livesText.text  = "LIVES  " + v; }
        void OnScore(int v)  { if (scoreText  != null) scoreText.text  = "SCORE  " + v.ToString("D8"); }
        void OnPower(int v)  { if (powerText  != null) powerText.text  = "POWER  Lv." + v; }
        void OnWeapon(WeaponData w) { if (weaponText != null) weaponText.text = "WEAPON  " + (w != null ? w.weaponName : "-"); }
    }
}
