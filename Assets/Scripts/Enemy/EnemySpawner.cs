using UnityEngine;

namespace ShootingGame.Enemy
{
    /// <summary>
    /// 임시 테스트 스포너. 일정 간격으로 화면 상단 임의 X에서 적을 떨어뜨린다.
    /// 정식 배치는 StageData 스폰 타임라인(§6.1)으로 대체 예정.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] EnemyData enemyData;
        [SerializeField] float interval = 1.2f;
        [Tooltip("스폰 X 범위(±, 월드 유닛)")]
        [SerializeField] float xRange = 3.5f;

        float timer;
        Camera cam;

        void Start()
        {
            cam = Camera.main;
            timer = 0.5f;
        }

        void Update()
        {
            if (enemyData == null || EnemyPool.Instance == null) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = interval;

            float topY = cam != null ? cam.transform.position.y + cam.orthographicSize + 0.5f : 6f;
            float x = Random.Range(-xRange, xRange);
            EnemyPool.Instance.Spawn(enemyData, new Vector3(x, topY, 0f));
        }
    }
}
