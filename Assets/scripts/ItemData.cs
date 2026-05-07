using UnityEngine;

public class ItemData : MonoBehaviour
{
    public string itemType;
    public enum ItemCategory { Weapon, Consumable }
    public ItemCategory category;
    public int amount = 1;
}