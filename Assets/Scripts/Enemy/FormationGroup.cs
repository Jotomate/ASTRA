using System.Collections.Generic;
using UnityEngine;
using ShootingGame.Core;

namespace ShootingGame.Enemy
{
    /// <summary>
    /// 편대 1무리를 추적해 전멸(전원 격파) 시 보너스를 지급한다. (GDD §5.8 wipeBonus)
    /// 화면밖으로 도망친 적이 있으면 전멸이 아니므로 보너스 없음.
    /// </summary>
    public class FormationGroup : MonoBehaviour
    {
        readonly List<Enemy> members = new List<Enemy>();
        int total, killed, bonus;
        bool done;
        float life;
        Enemy leader;
        bool scatterOnLeaderDeath;

        public void Init(List<Enemy> spawned, int bonusScore, bool leaderScatter)
        {
            members.AddRange(spawned);
            total = members.Count;
            bonus = bonusScore;
            scatterOnLeaderDeath = leaderScatter;
            if (members.Count > 0) leader = members[0];
            foreach (var e in members)
                if (e != null) e.OnKilled += OnMemberKilled;
        }

        void OnMemberKilled(Enemy e)
        {
            killed++;
            if (scatterOnLeaderDeath && e == leader) ScatterRest();
        }

        void ScatterRest()
        {
            Vector2 center = Vector2.zero;
            int alive = 0;
            foreach (var m in members)
                if (m != null && m.gameObject.activeSelf && !m.IsDead) { center += (Vector2)m.transform.position; alive++; }
            if (alive > 0) center /= alive;
            foreach (var m in members)
            {
                if (m == null || m == leader || !m.gameObject.activeSelf || m.IsDead) continue;
                Vector2 d = (Vector2)m.transform.position - center;
                if (d.sqrMagnitude < 0.01f) d = Random.insideUnitCircle;
                m.Scatter(d);
            }
        }

        void Update()
        {
            if (done) return;
            life += Time.deltaTime;

            bool anyActive = false;
            for (int i = 0; i < members.Count; i++)
            {
                var e = members[i];
                if (e != null && e.gameObject.activeSelf && !e.IsDead) { anyActive = true; break; }
            }

            if (!anyActive || life > 30f)
            {
                done = true;
                if (killed >= total && bonus > 0 && GameManager.Instance != null)
                    GameManager.Instance.AddScore(bonus);
                Destroy(gameObject);
            }
        }
    }
}
