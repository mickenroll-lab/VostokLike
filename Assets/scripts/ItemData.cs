using UnityEngine;

public class ItemData : MonoBehaviour
{
    public string itemType;
    public enum ItemCategory
    {
        Weapon,
        Consumable,
        Bullet,
        Magazine,
    }
    public ItemCategory category;
    public int amount = 1;
    public int value = 0;
    public int gridWidth = 1;
    public int gridHeight = 1;

    // Magazine専用フィールド
    public int maxAmmo = 0;
    public string compatibleWeapon = "";
    public string compatibleBullet = "";
}