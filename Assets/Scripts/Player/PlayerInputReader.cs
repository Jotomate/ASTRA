using UnityEngine;
using UnityEngine.InputSystem;

namespace ShootingGame.Player
{
    /// <summary>
    /// New Input System을 코드로 바인딩해 이동/발사 입력을 노출한다.
    /// 별도 .inputactions 에셋 없이 키보드(화살표·WASD)+게임패드를 지원.
    /// 발사 로직(무기 시스템)은 IsFiring을 구독해 연결한다.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        InputAction moveAction;
        InputAction fireAction;
        InputAction ejectAction;
        InputAction bombAction;

        /// <summary>정규화 전 이동 입력(-1..1). 대각선 보정은 소비 측에서.</summary>
        public Vector2 Move => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        /// <summary>발사 버튼 유지 여부.</summary>
        public bool IsFiring => fireAction != null && fireAction.IsPressed();
        /// <summary>이탈키를 이번 프레임에 눌렀는지(원샷). 장착 무기 해제 + 폭발 트리거.</summary>
        public bool EjectPressed => ejectAction != null && ejectAction.WasPressedThisFrame();
        /// <summary>봄(특수공격)을 이번 프레임에 눌렀는지(원샷).</summary>
        public bool BombPressed => bombAction != null && bombAction.WasPressedThisFrame();

        void Awake()
        {
            moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddBinding("<Gamepad>/leftStick");
            moveAction.AddBinding("<Gamepad>/dpad");

            fireAction = new InputAction("Fire", InputActionType.Button);
            fireAction.AddBinding("<Keyboard>/z");
            fireAction.AddBinding("<Keyboard>/space");
            fireAction.AddBinding("<Gamepad>/buttonSouth");

            // 이탈키 — 발사키와 분리 배치
            ejectAction = new InputAction("Eject", InputActionType.Button);
            ejectAction.AddBinding("<Keyboard>/x");
            ejectAction.AddBinding("<Keyboard>/leftShift");
            ejectAction.AddBinding("<Gamepad>/buttonEast");

            // 봄 — 특수공격
            bombAction = new InputAction("Bomb", InputActionType.Button);
            bombAction.AddBinding("<Keyboard>/c");
            bombAction.AddBinding("<Keyboard>/b");
            bombAction.AddBinding("<Gamepad>/buttonWest");
        }

        void OnEnable()
        {
            moveAction.Enable();
            fireAction.Enable();
            ejectAction.Enable();
            bombAction.Enable();
        }

        void OnDisable()
        {
            moveAction.Disable();
            fireAction.Disable();
            ejectAction.Disable();
            bombAction.Disable();
        }

        void OnDestroy()
        {
            moveAction?.Dispose();
            fireAction?.Dispose();
            ejectAction?.Dispose();
            bombAction?.Dispose();
        }
    }
}
