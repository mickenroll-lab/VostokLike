using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    public BoxContainer boxContainer;

    public void MoveToBox(string itemName, bool moveAll = false)
    {
        if (boxContainer == null || !boxGrid.activeSelf) return;

        if (moveAll)
        {
            int count = items.ContainsKey(itemName) ? items[itemName] : 0;
            boxContainer.AddToBox(itemName, count);
            items.Remove(itemName);
        }
        else
        {
            boxContainer.AddToBox(itemName, 1);
            RemoveItem(itemName);
        }

        // inventoryItemsからも削除
        InventoryItem invItem = inventoryItems.Find(i => i.itemName == itemName);
        if (invItem != null)
        {
            inventoryGrid.RemoveItem(invItem.itemId);
            inventoryItems.Remove(invItem);
        }

        UpdateInventoryUI();
        boxContainer.UpdateBoxUI();
    }

    public TextMeshProUGUI tooltipText;
    public GameObject boxGrid;
    public GameObject inventoryPanel;
    public GameObject gridCellPrefab;
    public Transform gridParent;
    public ItemManager itemManager;
    
    public GameObject crosshair;
    public GameObject dragGhostObject;

    public bool isOpen = false;

    private InventoryGrid inventoryGrid;
    private List<InventoryItem> inventoryItems = new List<InventoryItem>();

    private int gridWidth = 10;
    private int gridHeight = 12;

    // アイテム名と数量を管理
    public Dictionary<string, int> items = new Dictionary<string, int>();

    public bool justClosed = false;

    void Awake()
    {
        inventoryGrid = gameObject.AddComponent<InventoryGrid>();
        inventoryGrid.width = gridWidth;
        inventoryGrid.height = gridHeight;
        inventoryGrid.Initialize();
    }

    void Update()
    {
        if (justClosed)
        {
            justClosed = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.F) && isOpen)
        {
            isOpen = false;
            justClosed = true;
            inventoryPanel.SetActive(false);
            if (crosshair != null) crosshair.SetActive(true);
            tooltipText.gameObject.SetActive(false);
            if (boxGrid != null) boxGrid.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);
            if (crosshair != null) crosshair.SetActive(!isOpen);
            tooltipText.gameObject.SetActive(false);
            if (boxGrid != null) boxGrid.SetActive(false);
            if (isOpen)
                UpdateInventoryUI();
        }
    }

    public void OpenFromBox()
    {
        isOpen = true;
        inventoryPanel.SetActive(true);
        if (crosshair != null) crosshair.SetActive(false);
        UpdateInventoryUI();
    }
    public void AddItem(string itemName)
    {
        int stackLimit = GetStackLimit(itemName);

        // 既存のスタックで上限に達していないものを探す
        if (items.ContainsKey(itemName) && items[itemName] < stackLimit)
        {
            items[itemName]++;
        }
        else if (!items.ContainsKey(itemName))
        {
            items[itemName] = 1;
        }
        else
        {
            // 上限に達したら2つ目のスタックを作る
            string key = itemName + "_2";
            if (items.ContainsKey(key) && items[key] < stackLimit)
                items[key]++;
            else
                items[key] = 1;
        }
        // 新グリッドシステムにも追加
        int foundX, foundY;
        if (inventoryGrid.FindFreeSpace(1, 1, out foundX, out foundY))
        {
            InventoryItem newItem = new InventoryItem(itemName, foundX, foundY, 1, 1);
            inventoryItems.Add(newItem);
            inventoryGrid.PlaceItem(newItem.itemId, foundX, foundY, 1, 1);
        }
        Debug.Log("アイテム追加：" + itemName);
    }
    public void AddItem(string itemName, int w, int h)
    {
        // 既存のDictionary処理
        int stackLimit = GetStackLimit(itemName);
        if (items.ContainsKey(itemName) && items[itemName] < stackLimit)
            items[itemName]++;
        else if (!items.ContainsKey(itemName))
            items[itemName] = 1;
        else
        {
            string key = itemName + "_2";
            if (items.ContainsKey(key) && items[key] < stackLimit)
                items[key]++;
            else
                items[key] = 1;
        }

        // inventoryItemsにサイズ付きで追加
        int foundX, foundY;
        if (inventoryGrid.FindFreeSpace(w, h, out foundX, out foundY))
        {
            InventoryItem newItem = new InventoryItem(itemName, foundX, foundY, w, h);
            inventoryItems.Add(newItem);
            inventoryGrid.PlaceItem(newItem.itemId, foundX, foundY, w, h);
        }

        UpdateInventoryUI();
    }
    int GetStackLimit(string itemName)
    {
        if (itemName == "9x18mm") return 30;
        return 1;
    }

    public void UseItem(string itemName)
    {
        if (!items.ContainsKey(itemName)) return;

        if (itemName == "MedKit")
        {
            itemManager.PickupMedKit(30);
            items[itemName]--;
            if (items[itemName] <= 0)
                items.Remove(itemName);
            UpdateInventoryUI();
            Debug.Log("回復した");
        }
        else if (itemName == "BeefCan")
        {
            itemManager.PickupFood(30);
            items[itemName]--;
            if (items[itemName] <= 0)
                items.Remove(itemName);
            UpdateInventoryUI();
        }
        else if (itemName == "Water")
        {
            itemManager.PickupWater(30);
            items[itemName]--;
            if (items[itemName] <= 0)
                items.Remove(itemName);
            UpdateInventoryUI();
        }
    }


    public bool HasItem(string itemName)
    {
        return items.ContainsKey(itemName) && items[itemName] > 0;
    }

    public void RemoveItem(string itemName)
    {
        if (!items.ContainsKey(itemName)) return;
        items[itemName]--;
        if (items[itemName] <= 0)
            items.Remove(itemName);
        UpdateInventoryUI();
    }

    // ↓ここに追加
    public bool HasAmmo(string itemName)
    {
        if (items.ContainsKey(itemName) && items[itemName] > 0) return true;
        if (items.ContainsKey(itemName + "_2") && items[itemName + "_2"] > 0) return true;
        return false;
    }

    public void RemoveAmmo(string itemName)
    {
        if (items.ContainsKey(itemName) && items[itemName] > 0)
        {
            items[itemName]--;
            if (items[itemName] <= 0)
                items.Remove(itemName);
        }
        else if (items.ContainsKey(itemName + "_2") && items[itemName + "_2"] > 0)
        {
            items[itemName + "_2"]--;
            if (items[itemName + "_2"] <= 0)
                items.Remove(itemName + "_2");
        }
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(gridParent.GetChild(i).gameObject);

        int totalCells = gridWidth * gridHeight;

        // 空セルを先に全部生成
        for (int i = 0; i < totalCells; i++)
        {
            GameObject cell = Instantiate(gridCellPrefab, gridParent);
            cell.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        }

        // inventoryItemsから表示
        foreach (InventoryItem item in inventoryItems)
        {
            // 複数セルにわたって色を付ける
            for (int dx = 0; dx < item.gridWidth; dx++)
            {
                for (int dy = 0; dy < item.gridHeight; dy++)
                {
                    int cellX = item.gridX + dx;
                    int cellY = item.gridY + dy;
                    if (cellX >= gridWidth || cellY >= gridHeight) continue;

                    int cellIndex = cellY * gridWidth + cellX;
                    if (cellIndex >= gridParent.childCount) continue;

                    GameObject itemCell = gridParent.GetChild(cellIndex).gameObject;
                    itemCell.GetComponent<Image>().color = new Color(0.3f, 0.5f, 0.3f, 0.8f);

                        // メインセル（左上）にのみUI要素を追加
                        if (dx == 0 && dy == 0)

                        // メインセル以外にもDraggableItemをアタッチ
                        if (dx != 0 || dy != 0)
                        {
                            DraggableItem draggable = itemCell.AddComponent<DraggableItem>();
                            draggable.itemName = item.itemName;
                            draggable.fromInventory = true;
                            draggable.inventory = this;
                            draggable.boxContainer = boxContainer;
                            draggable.dragGhost = dragGhostObject;
                        }
                    {
                        TextMeshProUGUI text = itemCell.GetComponentInChildren<TextMeshProUGUI>();
                        if (text != null)
                        {
                            text.text = item.itemName == "9x18mm" && item.amount > 1 ? item.amount.ToString() : "";
                            text.fontSize = 28;
                            text.alignment = TextAlignmentOptions.BottomRight;
                        }

                        string captured = item.itemName;
                        itemCell.GetComponent<Button>().onClick.AddListener(() => {
                            if (Input.GetKey(KeyCode.LeftShift))
                                MoveToBox(captured, true);
                            else
                                UseItem(captured);
                        });

                        string tooltipName = item.itemName;
                        EventTrigger trigger = itemCell.AddComponent<EventTrigger>();

                        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                        enterEntry.eventID = EventTriggerType.PointerEnter;
                        enterEntry.callback.AddListener((data) => {
                            tooltipText.gameObject.SetActive(true);
                            tooltipText.text = tooltipName;
                        });
                        trigger.triggers.Add(enterEntry);

                        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                        exitEntry.eventID = EventTriggerType.PointerExit;
                        exitEntry.callback.AddListener((data) => {
                            tooltipText.gameObject.SetActive(false);
                        });
                        trigger.triggers.Add(exitEntry);

                        DraggableItem draggable = itemCell.AddComponent<DraggableItem>();
                        draggable.itemName = item.itemName;
                        draggable.fromInventory = true;
                        draggable.inventory = this;
                        draggable.boxContainer = boxContainer;
                        draggable.dragGhost = dragGhostObject;
                    }
                }
            }
        }
    }
    public int CalculateTotalValue()
    {
        int total = 0;
        foreach (var item in inventoryItems)
        {
            GameObject prefab = Resources.Load<GameObject>(item.itemName);
            if (prefab != null)
            {
                ItemData data = prefab.GetComponent<ItemData>();
                if (data != null)
                    total += data.value * item.amount;
            }
        }
        return total;
    }
}