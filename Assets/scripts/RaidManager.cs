using UnityEngine;

public class RaidManager : MonoBehaviour
{
    public static RaidManager Instance;

    public PlayerState playerState;
    public Inventory inventory;
    public SpawnManager spawnManager;
    public EnemySpawnManager enemySpawnManager;
    public ResultManager resultManager;

    public enum RaidState { SafeZone, InRaid }
    public RaidState currentState = RaidState.SafeZone;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void BeginRaid()
    {
        currentState = RaidState.InRaid;
        Debug.Log("レイド開始");
    }

    public void EndRaid(bool isDead)
    {
        currentState = RaidState.SafeZone;

        if (isDead)
        {
            inventory.ClearInventory();

            // ↓ 追加：装備スロットをクリア
            EquipmentSlot[] slots = FindObjectsOfType<EquipmentSlot>();
            foreach (EquipmentSlot slot in slots)
                slot.ForceUnequip();

            Debug.Log("レイド終了：死亡");
        }
        else
        {
            Debug.Log("レイド終了：帰還");
        }

        ResetRaidWorld();
        ReturnToSafeZone();
    }

    void ResetRaidWorld()
    {
        enemySpawnManager.ResetEnemies();
        Debug.Log("ワールドリセット完了");
    }

    void ReturnToSafeZone()
    {
        playerState.ResetState();
        spawnManager.SpawnAtSafe();
    }
}