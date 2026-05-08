using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LootTable", menuName = "Loot/LootTable")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public class LootEntry
    {
        public string itemName;
        public int minAmount;
        public int maxAmount;
        [Range(0, 100)]
        public float chance; // 出現確率（%）
    }

    public List<LootEntry> entries = new List<LootEntry>();
}