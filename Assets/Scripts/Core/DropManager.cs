using UnityEngine;
using ShootingGame.Enemy;
using ShootingGame.Item;

namespace ShootingGame.Core
{
    /// <summary>
    /// 적 처치 시 EnemyData의 드롭 설정(종류·확률)에 따라 파워업(P)/무기(W) 픽업을 생성한다. (GDD §5.1)
    /// </summary>
    public class DropManager : MonoBehaviour
    {
        public static DropManager Instance { get; private set; }

        [SerializeField] PowerUpPickup powerUpPrefab;
        [SerializeField] WeaponPickup weaponPickupPrefab;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SpawnDrop(EnemyData data, Vector3 position)
        {
            if (data == null || data.dropType == DropType.None) return;
            if (Random.value > data.dropChance) return;

            switch (data.dropType)
            {
                case DropType.PowerUp:
                    if (powerUpPrefab != null)
                        Instantiate(powerUpPrefab, position, Quaternion.identity);
                    break;

                case DropType.Weapon:
                    if (weaponPickupPrefab != null && data.weaponDrop != null)
                    {
                        WeaponPickup wp = Instantiate(weaponPickupPrefab, position, Quaternion.identity);
                        wp.SetWeapon(data.weaponDrop);
                    }
                    break;
            }
        }
    }
}
