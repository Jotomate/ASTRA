using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ShootingGame.Bullet
{
    /// <summary>
    /// 탄환 오브젝트 풀(필수). Instantiate/Destroy 남발 방지 → GC 스파이크 방지, 60fps 유지.
    /// 씬에 1개 배치. 자기탄/적탄 공용.
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        public static BulletPool Instance { get; private set; }

        [SerializeField] Bullet bulletPrefab;
        [SerializeField] int prewarm = 64;
        [SerializeField] int maxSize = 1024;

        ObjectPool<Bullet> pool;
        readonly List<Bullet> active = new List<Bullet>(128);

        /// <summary>현재 살아있는 탄환들(자기탄/적탄 혼재). 충돌 판정용. 순회 중 Despawn 시 역방향 루프 권장.</summary>
        public IReadOnlyList<Bullet> Active => active;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            pool = new ObjectPool<Bullet>(
                createFunc: CreateBullet,
                actionOnGet: b => b.gameObject.SetActive(true),
                actionOnRelease: b => b.gameObject.SetActive(false),
                actionOnDestroy: b => { if (b != null) Destroy(b.gameObject); },
                collectionCheck: false,
                defaultCapacity: prewarm,
                maxSize: maxSize);

            Prewarm();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        Bullet CreateBullet()
        {
            Bullet b = Instantiate(bulletPrefab, transform);
            b.SetPool(this);
            b.gameObject.SetActive(false);
            return b;
        }

        void Prewarm()
        {
            var tmp = new List<Bullet>(prewarm);
            for (int i = 0; i < prewarm; i++) tmp.Add(pool.Get());
            for (int i = 0; i < tmp.Count; i++) pool.Release(tmp[i]);
        }

        public Bullet Get()
        {
            Bullet b = pool.Get();
            active.Add(b);
            return b;
        }

        public void Release(Bullet b)
        {
            if (b == null) return;
            active.Remove(b);
            pool.Release(b);
        }
    }
}
