using UnityEngine;
using ShootingGame.Core;

namespace ShootingGame.Bullet
{
    /// <summary>
    /// 풀링되는 탄환. 속도 벡터로 직진하고 화면 밖으로 나가면 풀로 반환된다.
    /// 자기탄/적탄 공용. 옵션: 관통 / 벽 반사(maxBounces) / 유도(homing). 충돌은 원-원 판정 시스템이 Radius로 처리.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Bullet : MonoBehaviour
    {
        Vector2 velocity;
        float damage;
        float radius;
        bool isPlayerBullet;
        bool piercing;

        bool reflect;
        int bouncesLeft;
        bool homing;
        float homingTurn;
        float speed;
        Transform lockTarget;

        BulletPool pool;
        SpriteRenderer sr;
        Sprite defaultSprite;

        public float Damage => damage;
        public float Radius => radius;
        public bool IsPlayerBullet => isPlayerBullet;
        public bool Piercing => piercing;

        static Camera cam;
        const float OFFSCREEN_MARGIN = 1f;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            defaultSprite = sr.sprite;
        }

        public void SetPool(BulletPool p) => pool = p;

        public void Launch(Vector2 position, Vector2 vel, float dmg, float rad,
                           bool playerBullet, bool pierce, Color color, Sprite sprite,
                           bool doReflect = false, int maxBounces = 0,
                           bool doHoming = false, float homingTurnRate = 0f,
                           Transform homingTarget = null)
        {
            transform.position = position;
            velocity = vel;
            speed = vel.magnitude;
            damage = dmg;
            radius = rad;
            isPlayerBullet = playerBullet;
            piercing = pierce;
            reflect = doReflect;
            bouncesLeft = maxBounces;
            homing = doHoming;
            homingTurn = homingTurnRate;
            lockTarget = homingTarget;

            if (sr == null) sr = GetComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : defaultSprite;
            sr.color = color;
            FaceVelocity();
        }

        void Update()
        {
            float dt = Time.deltaTime;

            if (homing && speed > 0f)
            {
                var target = lockTarget != null
                    ? lockTarget
                    : (CollisionManager.Instance != null
                        ? CollisionManager.Instance.FindNearestTarget(transform.position, isPlayerBullet)
                        : null);
                if (target != null)
                {
                    Vector2 want = ((Vector2)target.position - (Vector2)transform.position).normalized;
                    Vector2 dir = Vector3.RotateTowards(velocity.normalized, want, homingTurn * Mathf.Deg2Rad * dt, 0f);
                    velocity = dir * speed;
                }
            }

            transform.position += (Vector3)(velocity * dt);

            if (reflect && bouncesLeft > 0) ReflectWalls();
            if (homing || reflect) FaceVelocity();

            if (IsOffscreen()) Despawn();
        }

        void ReflectWalls()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null || !cam.orthographic) return;
            float h = cam.orthographicSize, w = h * cam.aspect;
            Vector3 c = cam.transform.position;
            Vector3 p = transform.position;
            bool bounced = false;
            if (p.x < c.x - w) { p.x = c.x - w; velocity.x = Mathf.Abs(velocity.x); bounced = true; }
            else if (p.x > c.x + w) { p.x = c.x + w; velocity.x = -Mathf.Abs(velocity.x); bounced = true; }
            if (p.y > c.y + h) { p.y = c.y + h; velocity.y = -Mathf.Abs(velocity.y); bounced = true; }
            if (bounced) { transform.position = p; bouncesLeft--; }
        }

        void FaceVelocity()
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        bool IsOffscreen()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null || !cam.orthographic) return false;
            float h = cam.orthographicSize, w = h * cam.aspect;
            Vector3 c = cam.transform.position, p = transform.position;
            return p.y > c.y + h + OFFSCREEN_MARGIN || p.y < c.y - h - OFFSCREEN_MARGIN
                || p.x > c.x + w + OFFSCREEN_MARGIN || p.x < c.x - w - OFFSCREEN_MARGIN;
        }

        public void Despawn()
        {
            if (pool != null) pool.Release(this);
            else gameObject.SetActive(false);
        }
    }
}
