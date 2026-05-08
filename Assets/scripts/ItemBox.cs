using UnityEngine;
using System.Collections.Generic;

public class ItemBox : MonoBehaviour
{
    public LootTable lootTable;
    public Dictionary<string, int> contents = new Dictionary<string, int>();

    void Start()
    {
        GenerateLoot();
    }

    void GenerateLoot()
    {
        contents.Clear();
        if (lootTable == null) return;
        foreach (var entry in lootTable.entries)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= entry.chance)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                if (contents.ContainsKey(entry.itemName))
                    contents[entry.itemName] += amount;
                else
                    contents[entry.itemName] = amount;
            }
        }
    }
}