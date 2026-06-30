using UnityEngine;
using ShootingGame.Core;

namespace ShootingGame.Stage
{
    /// <summary>
    /// 파괴 가능한 지형 블록(하이브리드 지형 구간 대표 구현, §6). 아래로 스크롤하며
    /// 자기탄을 막아(피격·소멸) 데미지를 입고, 기체와 접촉 시 피해를 준다. 원-원 판정(IDamageable).
    /// ※ 풀 타일맵/이동 차단은 후속 작업.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TerrainBlock : MonoBehaviour, IDamageable
    {
        float hp, radius, scrollSpeed, flash;
        bool dead;
        Color baseColor;
        SpriteRenderer sr;
        static Camera cam;

        public Transform Transform => transform;
        public float HitRadius => radius;
        public bool IsDead => dead;

        public void Setup(float hp, float radius, float scroll, Sprite sprite, Color color, Material mat)
        {
            this.hp = hp; this.radius = radius; scrollSpeed = scroll; baseColor = color; dead = false; flash = 0f;
            sr = GetComponent<SpriteRenderer>();
            sr.sprite = sprite; sr.color = color; sr.sortingOrder = 6;
            if (mat != null) sr.sharedMaterial = mat;
            float d = radius * 2f;
            if (sprite != null) transform.localScale = Vector3.one * (d / Mathf.Max(0.01f, sprite.bounds.size.x));
            if (cam == null) cam = Camera.main;
            if (CollisionManager.Instance != null) CollisionManager.Instance.RegisterTarget(this);
        }

        void Update()
        {
            transform.position += Vector3.down * (scrollSpeed * Time.deltaTime);
            if (flash > 0f) { flash -= Time.deltaTime; sr.color = flash > 0f ? Color.white : baseColor; }
            if (cam != null && transform.position.y < cam.transform.position.y - cam.orthographicSize - radius - 1f)
                Despawn();
        }

        public void TakeDamage(float amount)
        {
            if (dead) return;
            hp -= amount;
            flash = 0.06f;
            if (AudioManager.Instance != null) AudioManager.Instance.Play("hit", 0.3f);
            if (hp <= 0f) Die();
        }

        void Die()
        {
            dead = true;
            if (CollisionManager.Instance != null) CollisionManager.Instance.UnregisterTarget(this);
            if (EffectPool.Instance != null) EffectPool.Instance.Play(transform.position, radius, new Color(0.8f, 0.7f, 0.5f, 1f));
            if (AudioManager.Instance != null) AudioManager.Instance.Play("explosion", 0.5f);
            if (GameManager.Instance != null) GameManager.Instance.AddKillScore(50);
            Destroy(gameObject);
        }

        void Despawn()
        {
            dead = true;
            if (CollisionManager.Instance != null) CollisionManager.Instance.UnregisterTarget(this);
            Destroy(gameObject);
        }
    }
}
