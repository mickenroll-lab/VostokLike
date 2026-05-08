using UnityEngine;

public class InventoryItem
{
    public string itemId;      // ユニークID
    public string itemName;    // アイテム名
    public int gridX;          // グリッド上のX座標
    public int gridY;          // グリッド上のY座標
    public int gridWidth;      // 占有幅
    public int gridHeight;     // 占有高さ
    public int amount;         // 数量（弾薬用）
    public int value;          // 価値

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