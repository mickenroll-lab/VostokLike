using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class CreateCorpseAssets
{
    [MenuItem("VostokLike/Create Corpse ContainerData")]
    static void Execute()
    {
        LootTable lootTable = ScriptableObject.CreateInstance<LootTable>();
        lootTable.entries = new List<LootTable.LootEntry>
        {
            new LootTable.LootEntry { itemName = "9x18mm",  minAmount = 5, maxAmount = 15, chance = 60f },
            new LootTable.LootEntry { itemName = "MedKit",  minAmount = 1, maxAmount = 1,  chance = 15f },
            new LootTable.LootEntry { itemName = "BeefCan", minAmount = 1, maxAmount = 1,  chance = 20f },
        };
        AssetDatabase.CreateAsset(lootTable, "Assets/ScriptableObjects/LootTable_Corpse.asset");

        ContainerData containerData = ScriptableObject.CreateInstance<ContainerData>();
        containerData.gridWidth  = 4;
        containerData.gridHeight = 4;
        containerData.emptyChance = 35f;
        containerData.lootTable = lootTable;
        AssetDatabase.CreateAsset(containerData, "Assets/ScriptableObjects/ContainerData_Corpse.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ContainerData_Corpse と LootTable_Corpse を Assets/ScriptableObjects/ に作成しました。");
    }
}
