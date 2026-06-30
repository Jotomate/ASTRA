using UnityEngine;

namespace ShootingGame.Effect
{
    /// <summary>
    /// 반경 폭발 비주얼. Play(pos, radius)로 호출하면 원이 반경까지 커지며 페이드아웃 후 비활성화.
    /// 씬에 1개 두고 재사용(이탈은 빈번하지 않아 풀링 불필요). 데미지는 호출 측(WeaponController)이 처리.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExplosionEffect : MonoBehaviour
    {
        [Tooltip("폭발 지속 시간(초)")]
        [SerializeField] float duration = 0.4f;
        [SerializeField] Color color = new Color(1f, 0.6f, 0.2f, 1f);

        SpriteRenderer sr;
        float spriteUnitDiameter = 1f;   // 스프라이트 1배 스케일에서의 월드 지름
        float timer;
        float targetRadius;
        bool playing;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr.sprite != null)
                spriteUnitDiameter = Mathf.Max(0.0001f, sr.sprite.bounds.size.x);
            gameObject.SetActive(false);
        }

        /// <summary>색상을 지정해 재생(풀링/격파 이펙트용).</summary>
        public void Play(Vector3 pos, float radius, Color tint)
        {
            color = tint;
            Play(pos, radius);
        }

        /// <summary>중심 pos에서 반경 radius까지 퍼지는 폭발 재생.</summary>
        public void Play(Vector3 pos, float radius)
        {
            transform.position = pos;
            targetRadius = radius;
            timer = 0f;
            playing = true;
            gameObject.SetActive(true);
            Apply(0f);
        }

        void Update()
        {
            if (!playing) return;

            timer += Time.deltaTime;
            float k = Mathf.Clamp01(timer / duration);
            Apply(k);

            if (k >= 1f)
            {
                playing = false;
                gameObject.SetActive(false);
            }
        }

        void Apply(float k)
        {
            // 지름 = 2*radius*k 가 되도록 스케일 환산
            float scale = (2f * targetRadius * k) / spriteUnitDiameter;
            transform.localScale = new Vector3(scale, scale, 1f);

            Color c = color;
            c.a = color.a * (1f - k);   // 커지면서 옅어짐
            sr.color = c;
        }
    }
}
