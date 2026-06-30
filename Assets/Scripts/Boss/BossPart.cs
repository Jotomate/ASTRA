using System;
using UnityEngine;
using ShootingGame.Core;
using ShootingGame.Bullet;

namespace ShootingGame.Boss
{
    /// <summary>
    /// 파괴 가능한 보스 부위(포탑/약점). 자체 체력·발사를 가지며 파괴되면 사라진다.
    /// Boss가 런타임에 생성·배치한다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BossPart : MonoBehaviour, IDamageable
    {
        BossPartDef def;
        SpriteRenderer sr;
        Transform player;
        float hp;
        float fireTimer;
        bool dead;

        public Transform Transform => transform;
        public float HitRadius => def != null ? def.hitRadius : 0.4f;
        public bool IsDead => dead;

        public Action<BossPart> Destroyed;

        public void Setup(BossPartDef d, Transform playerTransform, Material mat)
        {
            def = d;
            player = playerTransform;
            hp = d.hp;
            fireTimer = d.fireInterval;
            dead = false;

            sr = GetComponent<SpriteRenderer>();
            sr.sprite = d.sprite;
            sr.color = d.color;
            sr.sortingOrder = 12;
            if (mat != null) sr.sharedMaterial = mat;

            transform.localPosition = d.localPos;
            transform.localScale = Vector3.one * d.scale;

            if (CollisionManager.Instance != null) CollisionManager.Instance.RegisterTarget(this);
        }

        void Update()
        {
            if (dead || !def.canFire || BulletPool.Instance == null) return;
            fireTimer -= Time.deltaTime;
            if (fireTimer > 0f) return;
            fireTimer = def.fireInterval;

            Vector2 dir = player != null
                ? ((Vector2)player.position - (Vector2)transform.position).normalized
                : Vector2.down;
            var b = BulletPool.Instance.Get();
            b.Launch(transform.position, dir * def.bulletSpeed, def.bulletDamage,
                     def.bulletRadius, false, false, def.bulletColor, null);
        }

        public void TakeDamage(float amount)
        {
            if (dead) return;
            hp -= amount;
            if (hp <= 0f) Kill();
        }

        /// <summary>보스 사망 시 외부에서 강제 정리.</summary>
        public void ForceRemove()
        {
            if (dead) return;
            dead = true;
            if (CollisionManager.Instance != null) CollisionManager.Instance.UnregisterTarget(this);
            gameObject.SetActive(false);
        }

        void Kill()
        {
            dead = true;
            if (CollisionManager.Instance != null) CollisionManager.Instance.UnregisterTarget(this);
            if (GameManager.Instance != null) GameManager.Instance.AddKillScore(def.score);
            Destroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
