using UnityEngine;
using UnityEngine.UI;
using ShootingGame.Core;

namespace ShootingGame.UI
{
    /// <summary>
    /// 콤보 배율이 오르는 순간 "x2!" 팝업을 크게 띄웠다가 축소·페이드. GameManager.ComboChanged 구독.
    /// unscaledDeltaTime 사용(히트스톱 중에도 애니메이션 진행).
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class ComboPopup : MonoBehaviour
    {
        [Tooltip("팝업 애니메이션 시간(초)")]
        [SerializeField] float duration = 0.5f;
        [Tooltip("등장 시 확대 배율")]
        [SerializeField] float popScale = 1.7f;

        Text text;
        RectTransform rt;
        float timer;
        bool playing;
        int lastMult = 1;

        void Awake()
        {
            text = GetComponent<Text>();
            rt = GetComponent<RectTransform>();
            SetAlpha(0f);
        }

        void Start()
        {
            if (GameManager.Instance != null) GameManager.Instance.ComboChanged += OnCombo;
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.ComboChanged -= OnCombo;
        }

        void OnCombo(int combo, int mult)
        {
            if (combo == 0) { lastMult = 1; return; }   // 리셋
            if (mult > lastMult && combo >= 2)
            {
                text.text = "x" + mult + "!";
                timer = 0f;
                playing = true;
            }
            lastMult = mult;
        }

        void Update()
        {
            if (!playing) return;
            timer += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(timer / duration);
            float s = Mathf.Lerp(popScale, 1f, k);
            rt.localScale = new Vector3(s, s, 1f);
            SetAlpha(1f - k);
            if (k >= 1f) { playing = false; SetAlpha(0f); }
        }

        void SetAlpha(float a)
        {
            Color c = text.color; c.a = a; text.color = c;
        }
    }
}
