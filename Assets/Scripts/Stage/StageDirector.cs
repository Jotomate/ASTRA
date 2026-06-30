using System.Collections.Generic;
using UnityEngine;
using ShootingGame.Enemy;
using ShootingGame.Core;
using BossEntity = ShootingGame.Boss.Boss;
using BossCard = ShootingGame.Boss.BossData;
using EnemyEntity = ShootingGame.Enemy.Enemy;

namespace ShootingGame.Stage
{
    /// <summary>
    /// StageData 스폰 타임라인을 실행한다: 시간순으로 적/편대/보스경고/보스를 스폰하고,
    /// 모든 이벤트 종료 + (보스 격파 or 보스 없음) 시 스테이지 클리어를 통지한다. (§6.1)
    /// </summary>
    public class StageDirector : MonoBehaviour
    {
        [SerializeField] StageData stage;
        [Tooltip("보스 본체 Unlit 머티리얼")]
        [SerializeField] Material spriteMaterial;
        [SerializeField] float spawnTopMargin = 0.6f;
        [Tooltip("지형 블록 스프라이트")]
        [SerializeField] Sprite terrainSprite;

        [Header("멀티 스테이지")]
        [Tooltip("클리어 후 진행할 다음 스테이지(없으면 loop에 따라 현재 반복)")]
        [SerializeField] StageData nextStage;
        [SerializeField] bool loop = true;
        [SerializeField] float clearToNextDelay = 4f;

        readonly List<SpawnEvent> events = new List<SpawnEvent>();
        float stageTime, clearTimer;
        int nextEvent;
        bool bossSpawned, bossDefeated, stageCleared;
        Camera cam;

        void Start()
        {
            cam = Camera.main;
            LoadTimeline();
            if (GameManager.Instance != null) GameManager.Instance.BossDefeated += OnBossDefeated;
        }

        void LoadTimeline()
        {
            events.Clear();
            if (stage != null && stage.spawnEvents != null)
            {
                events.AddRange(stage.spawnEvents);
                events.Sort((a, b) => a.time.CompareTo(b.time));
            }
            stageTime = 0f; clearTimer = 0f; nextEvent = 0;
            bossSpawned = false; bossDefeated = false; stageCleared = false;
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.BossDefeated -= OnBossDefeated;
        }

        void Update()
        {
            if (stageCleared)
            {
                clearTimer += Time.deltaTime;
                if ((nextStage != null || loop) && clearTimer >= clearToNextDelay)
                    AdvanceStage();
                return;
            }

            stageTime += Time.deltaTime;
            while (nextEvent < events.Count && events[nextEvent].time <= stageTime)
            {
                Fire(events[nextEvent]);
                nextEvent++;
            }

            if (nextEvent >= events.Count && (!bossSpawned || bossDefeated))
            {
                stageCleared = true;
                if (GameManager.Instance != null) GameManager.Instance.NotifyStageClear();
            }
        }

        float TopY => cam != null ? cam.transform.position.y + cam.orthographicSize + spawnTopMargin : 6f;

        void Fire(SpawnEvent e)
        {
            switch (e.kind)
            {
                case SpawnKind.Enemy:
                    if (e.enemy != null && EnemyPool.Instance != null)
                    {
                        float jitter = e.spawnXJitter > 0f ? Random.Range(-e.spawnXJitter, e.spawnXJitter) : 0f;
                        EnemyPool.Instance.Spawn(e.enemy, new Vector3(e.spawnX + jitter, TopY, 0f));
                    }
                    break;

                case SpawnKind.Formation:
                    SpawnFormation(e.formation, e.spawnX);
                    break;

                case SpawnKind.BossWarning:
                    if (GameManager.Instance != null) GameManager.Instance.NotifyBossWarning();
                    break;

                case SpawnKind.Boss:
                    SpawnBoss(e.boss);
                    break;

                case SpawnKind.Terrain:
                    SpawnTerrain(e.spawnX);
                    break;
            }
        }

        void SpawnTerrain(float x)
        {
            if (terrainSprite == null) return;
            var go = new GameObject("Terrain");
            go.transform.position = new Vector3(x, TopY, 0f);
            var tb = go.AddComponent<TerrainBlock>();
            float scroll = stage != null ? stage.baseScrollSpeed : 2.5f;
            tb.Setup(8f, 0.8f, scroll, terrainSprite, new Color(0.55f, 0.5f, 0.45f, 1f), spriteMaterial);
        }

        void SpawnFormation(FormationData f, float baseX)
        {
            if (f == null || f.enemyData == null || EnemyPool.Instance == null) return;
            float topY = TopY;
            var spawned = new List<EnemyEntity>(f.count);
            for (int i = 0; i < f.count; i++)
            {
                Vector2 off = FormationOffset(f, i);
                var e = EnemyPool.Instance.Spawn(f.enemyData, new Vector3(baseX + off.x, topY + off.y, 0f));
                if (e != null) spawned.Add(e);
            }
            if (spawned.Count > 0 && (f.wipeBonus || f.leaderBreakScatter))
            {
                var go = new GameObject("FormationGroup");
                go.AddComponent<FormationGroup>().Init(spawned, f.wipeBonus ? f.count * 200 : 0, f.leaderBreakScatter);
            }
        }

        static Vector2 FormationOffset(FormationData f, int i)
        {
            int n = Mathf.Max(1, f.count);
            float s = f.spacing;
            float half = (n - 1) / 2f;
            switch (f.pattern)
            {
                case FormationPattern.Column:
                    return new Vector2(0f, i * s);
                case FormationPattern.VShape:
                    return new Vector2((i - half) * s, Mathf.Abs(i - half) * s * 0.7f);
                default: // Line
                    return new Vector2((i - half) * s, 0f);
            }
        }

        void SpawnBoss(BossCard bd)
        {
            if (bd == null) return;
            bossSpawned = true;
            var go = new GameObject("Boss_" + bd.bossName);
            Vector3 pos = new Vector3(0f, TopY + 0.5f, 0f);
            var boss = go.AddComponent<BossEntity>();
            boss.Setup(bd, pos, spriteMaterial);
        }

        void AdvanceStage()
        {
            if (nextStage != null) stage = nextStage;
            LoadTimeline();
            if (GameManager.Instance != null) GameManager.Instance.NextStage();
        }

        void OnBossDefeated() => bossDefeated = true;
    }
}
