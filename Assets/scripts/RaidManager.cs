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
        Debug.Log("���C�h�J�n");
    }

    public void EndRaid(bool isDead)
    {
        currentState = RaidState.SafeZone;

        if (isDead)
        {
            inventory.ClearInventory();

            // �� �ǉ��F�����X���b�g���N���A
            EquipmentSlot[] slots = FindObjectsOfType<EquipmentSlot>();
            foreach (EquipmentSlot slot in slots)
                slot.ForceUnequip();

            Debug.Log("���C�h�I���F���S");
        }
        else
        {
            Debug.Log("���C�h�I���F�A��");
        }

        ResetRaidWorld();
        ReturnToSafeZone();
    }

    void ResetRaidWorld()
    {
        enemySpawnManager.ResetEnemies();

        LootContainer[] containers = FindObjectsOfType<LootContainer>();
        foreach (LootContainer container in containers)
            container.ResetContainer();

        Debug.Log("ワールドリセット完了");
    }

    void ReturnToSafeZone()
    {
        playerState.ResetState();
        spawnManager.SpawnAtSafe();
    }
}