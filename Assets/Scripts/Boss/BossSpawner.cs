using UnityEngine;

namespace ShootingGame.Boss
{
    /// <summary>
    /// 임시 보스 스포너. spawnDelay 후(또는 SpawnNow 호출) 보스를 화면 위에서 등장시킨다.
    /// 정식 등장은 StageData 스폰 타임라인(§6.1)으로 대체 예정.
    /// </summary>
    public class BossSpawner : MonoBehaviour
    {
        [SerializeField] BossData bossData;
        [SerializeField] Material spriteMaterial;
        [SerializeField] bool spawnOnStart = true;
        [SerializeField] float spawnDelay = 5f;
        [SerializeField] float spawnX = 0f;
        [SerializeField] float spawnY = 7f;

        bool spawned;
        float timer;

        void Start() => timer = spawnDelay;

        void Update()
        {
            if (!spawnOnStart || spawned || bossData == null) return;
            timer -= Time.deltaTime;
            if (timer <= 0f) SpawnNow();
        }

        public void SpawnNow()
        {
            if (spawned || bossData == null) return;
            spawned = true;

            var go = new GameObject("Boss_" + bossData.bossName);
            Vector3 pos = new Vector3(spawnX, spawnY, 0f);
            var boss = go.AddComponent<Boss>();
            boss.Setup(bossData, pos, spriteMaterial);
        }
    }
}
