using UnityEngine;

namespace ShootingGame.Player
{
    /// <summary>
    /// 좌/우 이동 입력에 따라 기체 스프라이트를 뱅킹(기울임) 버전으로 전환한다.
    /// 뱅킹 스프라이트가 비어 있으면 기본 스프라이트를 유지(안전).
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerBank : MonoBehaviour
    {
        [SerializeField] Sprite neutral;
        [SerializeField] Sprite bankLeft;
        [SerializeField] Sprite bankRight;
        [Tooltip("이 값보다 작은 입력은 중립으로 간주")]
        [SerializeField] float deadzone = 0.2f;

        PlayerInputReader input;
        SpriteRenderer sr;

        void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            sr = GetComponent<SpriteRenderer>();
            if (neutral == null) neutral = sr.sprite;
        }

        void Update()
        {
            float x = input.Move.x;
            Sprite target = neutral;
            if (x < -deadzone) target = bankLeft != null ? bankLeft : neutral;
            else if (x > deadzone) target = bankRight != null ? bankRight : neutral;

            if (sr.sprite != target) sr.sprite = target;
        }
    }
}
