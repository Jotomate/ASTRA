using UnityEngine;

namespace ShootingGame.Player
{
    /// <summary>
    /// 부활 무적(IsInvulnerable) 동안 기체 스프라이트를 점멸시키는 연출.
    /// timeScale 영향을 받지 않도록 unscaledTime 사용.
    /// </summary>
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerBlink : MonoBehaviour
    {
        [SerializeField] float blinkInterval = 0.09f;
        [SerializeField] float dimAlpha = 0.25f;

        Player player;
        SpriteRenderer sr;

        void Awake()
        {
            player = GetComponent<Player>();
            sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            float a = 1f;
            if (player.IsInvulnerable)
                a = (Mathf.FloorToInt(Time.unscaledTime / blinkInterval) % 2 == 0) ? dimAlpha : 1f;

            Color c = sr.color;
            if (!Mathf.Approximately(c.a, a))
            {
                c.a = a;
                sr.color = c;
            }
        }
    }
}
