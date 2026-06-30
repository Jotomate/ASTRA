using UnityEngine;

namespace ShootingGame.Core
{
    /// <summary>카메라 흔들림. Main Camera에 부착. unscaledDeltaTime 사용(히트스톱/정지 무관).</summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        Vector3 basePos;
        float timer, duration, amplitude;

        void Awake()
        {
            Instance = this;
            basePos = transform.localPosition;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>흔들기(누적 시 더 큰 값 채택).</summary>
        public void Shake(float amp, float dur)
        {
            amplitude = Mathf.Max(amplitude, amp);
            duration = Mathf.Max(duration, dur);
            timer = 0f;
        }

        void LateUpdate()
        {
            if (duration <= 0f) { transform.localPosition = basePos; return; }
            timer += Time.unscaledDeltaTime;
            if (timer >= duration) { duration = 0f; amplitude = 0f; transform.localPosition = basePos; return; }
            float falloff = 1f - timer / duration;
            Vector2 o = Random.insideUnitCircle * (amplitude * falloff);
            transform.localPosition = basePos + new Vector3(o.x, o.y, 0f);
        }
    }
}
