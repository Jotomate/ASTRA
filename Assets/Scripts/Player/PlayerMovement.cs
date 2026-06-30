using UnityEngine;

namespace ShootingGame.Player
{
    /// <summary>
    /// Transform 직접 이동 + 플레이 영역(카메라 뷰) 클램프.
    /// 충돌은 Unity 물리가 아닌 원-원 거리 비교(별도 시스템)로 처리하므로 여기서는 위치만 다룬다.
    /// </summary>
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerMovement : MonoBehaviour
    {
        Player player;
        PlayerInputReader input;
        Camera cam;

        PlayerData Data => player.Data;

        void Awake()
        {
            player = GetComponent<Player>();
            input = GetComponent<PlayerInputReader>();
            cam = Camera.main;
        }

        void Update()
        {
            // 대각선 속도 보정: 크기 1로 제한(과속 방지), 아날로그 입력의 미세 이동은 보존.
            Vector2 dir = Vector2.ClampMagnitude(input.Move, 1f);
            Vector3 delta = (Vector3)(dir * (Data.moveSpeed * Time.deltaTime));
            transform.position = ClampToScreen(transform.position + delta);
        }

        Vector3 ClampToScreen(Vector3 pos)
        {
            if (cam == null || !cam.orthographic) return pos;

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            Vector3 c = cam.transform.position;
            float pad = Data.screenPadding;

            pos.x = Mathf.Clamp(pos.x, c.x - halfW + pad, c.x + halfW - pad);
            pos.y = Mathf.Clamp(pos.y, c.y - halfH + pad, c.y + halfH - pad);
            return pos;
        }
    }
}
