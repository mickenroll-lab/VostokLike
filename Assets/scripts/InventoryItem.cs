using UnityEngine;

public class InventoryItem
{
    public string itemId;
    public string itemName;
    public int gridX;
    public int gridY;
    public int gridWidth;
    public int gridHeight;
    public int amount;
    public int value;
    // 残弾数（デフォルト0）。将来はマガジンScriptableObjectに置き換え予定。
    public int ammo = 0;

    public InventoryItem(string name, int x, int y, int w, int h, int amt = 1, int val = 0)
    {
        itemId = System.Guid.NewGuid().ToString();
        itemName = name;
        gridX = x;
        gridY = y;
        gridWidth = w;
        gridHeight = h;
        amount = amt;
        value = val;
    }
}
