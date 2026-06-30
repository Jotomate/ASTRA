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

        public void Init(List<Enemy> spawned, int bonusScore)
        {
            members.AddRange(spawned);
            total = members.Count;
            bonus = bonusScore;
            foreach (var e in members)
                if (e != null) e.OnKilled += OnMemberKilled;
        }

        void OnMemberKilled(Enemy e) => killed++;

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
