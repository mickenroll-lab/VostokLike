using UnityEngine;

public class RaidManager : MonoBehaviour
{
    public static RaidManager Instance;

    public PlayerState playerState;
    public Inventory inventory;
    public EquipmentSlot[] equipmentSlots;
    public SpawnManager spawnManager;
    public EnemySpawnManager enemySpawnManager;
    public ResultManager resultManager;
    public Transform raidRuntimeRoot;

    public enum RaidState { SafeZone, InRaid }
    public RaidState currentState = RaidState.SafeZone;
    public float raidStartTime = 0f;

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
        raidStartTime = TimeManager.Instance?.currentTime ?? 0f;
        Debug.Log("レイド開始");
    }

    public void EndRaid(bool isDead)
    {
        currentState = RaidState.SafeZone;

        if (isDead)
        {
            inventory.ClearInventory();

            // �� �ǉ��F�����X���b�g���N���A
            foreach (EquipmentSlot slot in equipmentSlots)
                if (slot != null) slot.ForceUnequip();

            Debug.Log("���C�h�I���F���S");
        }
        else
        {
            Debug.Log("���C�h�I���F�A��");
        }

        ResetRaidWorld();
        ReturnToSafeZone(isDead);
    }

    void ResetRaidWorld()
    {
        enemySpawnManager.ResetEnemies();

        if (raidRuntimeRoot != null)
        {
            Transform lootRoot = raidRuntimeRoot.Find("Loot");
            if (lootRoot != null)
            {
                foreach (LootContainer container in lootRoot.GetComponentsInChildren<LootContainer>())
                    container.ResetContainer();
            }
        }
        else
        {
            foreach (LootContainer container in FindObjectsOfType<LootContainer>())
                container.ResetContainer();
        }

        Debug.Log("ワールドリセット完了");
    }

    void ReturnToSafeZone(bool isDead)
    {
        if (isDead)
            playerState.ResetState();
        else
            playerState.ResumeAfterRaid();
        spawnManager.SpawnAtSafe();
    }
}