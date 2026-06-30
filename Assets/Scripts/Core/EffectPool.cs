using UnityEngine;
using ShootingGame.Effect;

namespace ShootingGame.Core
{
    /// <summary>격파 폭발 이펙트 풀. 런타임에 ExplosionEffect 풀을 만들어 라운드로빈 재생.</summary>
    public class EffectPool : MonoBehaviour
    {
        public static EffectPool Instance { get; private set; }

        [SerializeField] Sprite explosionSprite;
        [SerializeField] Material spriteMaterial;
        [SerializeField] int poolSize = 20;

        ExplosionEffect[] pool;
        int idx;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            pool = new ExplosionEffect[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject("Explosion");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = explosionSprite;
                if (spriteMaterial != null) sr.sharedMaterial = spriteMaterial;
                sr.sortingOrder = 25;
                pool[i] = go.AddComponent<ExplosionEffect>();   // Awake에서 비활성화
            }
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Play(Vector3 pos, float radius, Color tint)
        {
            if (pool == null || pool.Length == 0) return;
            var e = pool[idx];
            idx = (idx + 1) % pool.Length;
            e.Play(pos, radius, tint);
        }
    }
}
