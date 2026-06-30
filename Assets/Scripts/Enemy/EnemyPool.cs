using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ShootingGame.Enemy
{
    /// <summary>
    /// 적 오브젝트 풀(필수). 단일 Enemy 프리팹을 EnemyData로 구성해 재사용 → 적 종류는 카드로 무한 확장.
    /// </summary>
    public class EnemyPool : MonoBehaviour
    {
        public static EnemyPool Instance { get; private set; }

        [SerializeField] Enemy enemyPrefab;
        [SerializeField] int prewarm = 32;
        [SerializeField] int maxSize = 512;

        ObjectPool<Enemy> pool;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            pool = new ObjectPool<Enemy>(
                createFunc: CreateEnemy,
                actionOnGet: e => { },                          // 활성화는 Spawn()에서
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => { if (e != null) Destroy(e.gameObject); },
                collectionCheck: false,
                defaultCapacity: prewarm,
                maxSize: maxSize);

            Prewarm();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        Enemy CreateEnemy()
        {
            Enemy e = Instantiate(enemyPrefab, transform);
            e.SetPool(this);
            e.gameObject.SetActive(false);
            return e;
        }

        void Prewarm()
        {
            var tmp = new List<Enemy>(prewarm);
            for (int i = 0; i < prewarm; i++) tmp.Add(pool.Get());
            for (int i = 0; i < tmp.Count; i++) pool.Release(tmp[i]);
        }

        /// <summary>EnemyData로 적 1기 스폰.</summary>
        public Enemy Spawn(EnemyData data, Vector3 position)
        {
            Enemy e = pool.Get();
            e.Spawn(data, position);
            return e;
        }

        public void Release(Enemy e)
        {
            if (e != null) pool.Release(e);
        }
    }
}
