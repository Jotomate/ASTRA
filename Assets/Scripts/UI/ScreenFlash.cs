using UnityEngine;
using UnityEngine.UI;

namespace ShootingGame.UI
{
    /// <summary>
    /// 전체 화면 색 플래시. HUD Canvas의 전체 덮개 Image에 부착. Flash(color, peak, dur)로 명멸.
    /// unscaledDeltaTime 사용 → 히트스톱/일시정지 중에도 진행. 다중 호출 시 더 강한 값 채택.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ScreenFlash : MonoBehaviour
    {
        public static ScreenFlash Instance { get; private set; }

        Image img;
        Color baseColor = Color.white;
        float timer, duration, peakAlpha;

        void Awake()
        {
            Instance = this;
            img = GetComponent<Image>();
            img.raycastTarget = false;
            SetAlpha(0f);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>color 색으로 peak 알파까지 번쩍였다가 dur초에 걸쳐 사라짐.</summary>
        public void Flash(Color color, float peak = 0.5f, float dur = 0.25f)
        {
            baseColor = color;
            peakAlpha = Mathf.Max(peakAlpha, peak);
            duration = Mathf.Max(duration, dur);
            timer = 0f;
        }

        void Update()
        {
            if (duration <= 0f) return;
            timer += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(timer / duration);
            SetAlpha(peakAlpha * (1f - k));
            if (k >= 1f) { duration = 0f; peakAlpha = 0f; SetAlpha(0f); }
        }

        void SetAlpha(float a)
        {
            Color c = baseColor; c.a = a;
            if (img != null) img.color = c;
        }
    }
}
