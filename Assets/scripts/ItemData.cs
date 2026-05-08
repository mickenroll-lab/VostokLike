using UnityEngine;

public class ItemData : MonoBehaviour
{
    public string itemType;
    public enum ItemCategory { Weapon, Consumable }
    public ItemCategory category;
    public int amount = 1;
    public int value = 0;
    public int gridWidth = 1;  // グリッド上の幅
    public int gridHeight = 1; // グリッド上の高さ
}