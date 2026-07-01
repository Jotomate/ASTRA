using UnityEngine;

namespace ShootingGame.Effect
{
    /// <summary>
    /// 발사 순간 총구 섬광. 플레이어 자식으로 두고 Flash()로 재점멸한다(짧게 명멸하므로 풀링 불필요, 재트리거).
    /// 스프라이트/머티리얼은 런타임 절차 생성(에셋 의존 없음). URP 2D 무광 표시를 위해 Sprites/Default 사용.
    /// </summary>
    public class MuzzleFlash : MonoBehaviour
    {
        [Tooltip("섬광 지속 시간(초)")]
        [SerializeField] float duration = 0.06f;
        [Tooltip("기본 크기(월드 유닛, 지름)")]
        [SerializeField] float baseScale = 0.55f;

        SpriteRenderer sr;
        Color color = new Color(1f, 0.95f, 0.7f, 1f);
        float timer, scale;
        bool playing;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            if (sr.sprite == null) sr.sprite = BuildGlow();
            var sh = Shader.Find("Sprites/Default");
            if (sh != null) sr.sharedMaterial = new Material(sh);
            sr.sortingOrder = 20;
            SetAlpha(0f);
        }

        /// <summary>총구 위치 pos에서 tint 색으로 섬광. scaleMul로 차지샷 등 크게.</summary>
        public void Flash(Vector3 pos, Color tint, float scaleMul = 1f)
        {
            transform.position = pos;
            color = tint; color.a = 1f;
            scale = baseScale * scaleMul;
            timer = 0f;
            playing = true;
            if (!gameObject.activeSelf) gameObject.SetActive(true);
        }

        void Update()
        {
            if (!playing) return;
            timer += Time.deltaTime;
            float k = Mathf.Clamp01(timer / duration);
            float s = scale * (0.6f + 0.4f * (1f - k));   // 살짝 수축
            transform.localScale = new Vector3(s, s, 1f);
            SetAlpha(1f - k);
            if (k >= 1f) playing = false;
        }

        void SetAlpha(float a)
        {
            Color c = color; c.a = a;
            if (sr != null) sr.color = c;
        }

        /// <summary>중심이 밝고 가장자리로 사라지는 부드러운 원형 글로우 스프라이트(지름 1유닛).</summary>
        static Sprite BuildGlow()
        {
            const int R = 32;
            var tex = new Texture2D(R, R, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            Vector2 c = new Vector2(R * 0.5f, R * 0.5f);
            for (int y = 0; y < R; y++)
                for (int x = 0; x < R; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c) / (R * 0.5f);
                    float a = Mathf.Clamp01(1f - d);
                    a *= a;   // 부드러운 감쇠
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, R, R), new Vector2(0.5f, 0.5f), R);
        }
    }
}
