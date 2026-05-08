using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;
    public GameObject boxGrid;
    public GameObject inventoryPanel;
    public GameObject gridCellPrefab;
    public Transform gridParent;
    public ItemManager itemManager;
    public GameObject crosshair;
    public BoxContainer boxContainer;
    public GameObject dragGhostObject;

    public bool isOpen = false;
    public bool justClosed = false;

    private int gridWidth = 10;
    private int gridHeight = 12;

    private InventoryGrid inventoryGrid;
    private List<InventoryItem> items = new List<InventoryItem>();
    private GameObject[,] cellObjects;

    void Awake()
    {
        inventoryGrid = gameObject.AddComponent<InventoryGrid>();
        inventoryGrid.width = gridWidth;
        inventoryGrid.height = gridHeight;
        inventoryGrid.Initialize();
        cellObjects = new GameObject[gridWidth, gridHeight];
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

    public bool AddItem(string itemName, int w = 1, int h = 1, int amount = 1, int value = 0)
    {
        // 弾薬はスタック処理
        if (itemName == "9x18mm")
        {
            InventoryItem existing = items.Find(i => i.itemName == "9x18mm");
            if (existing != null && existing.amount < 30)
            {
                existing.amount = Mathf.Min(existing.amount + amount, 30);
                UpdateInventoryUI();
                return true;
            }
        }

        // 空きスペースを探す
        int foundX, foundY;
        if (!inventoryGrid.FindFreeSpace(w, h, out foundX, out foundY))
        {
            Debug.Log("インベントリが満杯");
            return false;
        }

        InventoryItem newItem = new InventoryItem(itemName, foundX, foundY, w, h, amount, value);
        items.Add(newItem);
        inventoryGrid.PlaceItem(newItem.itemId, foundX, foundY, w, h);
        UpdateInventoryUI();
        return true;
    }

    public bool RemoveItem(string itemName)
    {
        InventoryItem item = items.Find(i => i.itemName == itemName);
        if (item == null) return false;

        inventoryGrid.RemoveItem(item.itemId);
        items.Remove(item);
        UpdateInventoryUI();
        return true;
    }

    public bool HasItem(string itemName)
    {
        return items.Exists(i => i.itemName == itemName);
    }

    public bool HasAmmo(string itemName)
    {
        InventoryItem item = items.Find(i => i.itemName == itemName);
        return item != null && item.amount > 0;
    }

    public void RemoveAmmo(string itemName)
    {
        InventoryItem item = items.Find(i => i.itemName == itemName);
        if (item == null) return;

        item.amount--;
        if (item.amount <= 0)
        {
            inventoryGrid.RemoveItem(item.itemId);
            items.Remove(item);
        }
        UpdateInventoryUI();
    }

    public int CalculateTotalValue()
    {
        int total = 0;
        foreach (var item in items)
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

    public void UseItem(string itemId)
    {
        InventoryItem item = items.Find(i => i.itemId == itemId);
        if (item == null) return;

        if (item.itemName == "MedKit")
        {
            itemManager.PickupMedKit(30);
            inventoryGrid.RemoveItem(item.itemId);
            items.Remove(item);
            UpdateInventoryUI();
        }
        else if (item.itemName == "BeefCan")
        {
            itemManager.PickupFood(30);
            inventoryGrid.RemoveItem(item.itemId);
            items.Remove(item);
            UpdateInventoryUI();
        }
        else if (item.itemName == "Water")
        {
            itemManager.PickupWater(30);
            inventoryGrid.RemoveItem(item.itemId);
            items.Remove(item);
            UpdateInventoryUI();
        }
    }

    public void MoveToBox(string itemId, bool moveAll = false)
    {
        if (boxContainer == null || !boxGrid.activeSelf) return;

        InventoryItem item = items.Find(i => i.itemId == itemId);
        if (item == null) return;

        boxContainer.AddToBox(item.itemName, item.amount);
        inventoryGrid.RemoveItem(item.itemId);
        items.Remove(item);
        UpdateInventoryUI();
        boxContainer.UpdateBoxUI();
    }

    public void UpdateInventoryUI()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        int totalCells = gridWidth * gridHeight;

        // 空セルを先に全部生成
        for (int i = 0; i < totalCells; i++)
        {
            GameObject cell = Instantiate(gridCellPrefab, gridParent);
            cell.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        }

        // アイテムを配置
        foreach (InventoryItem item in items)
        {
            int cellIndex = item.gridY * gridWidth + item.gridX;
            if (cellIndex >= gridParent.childCount) continue;

            GameObject itemCell = gridParent.GetChild(cellIndex).gameObject;
            Image cellImage = itemCell.GetComponent<Image>();
            cellImage.color = new Color(0.3f, 0.5f, 0.3f, 0.8f);

            TextMeshProUGUI text = itemCell.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = item.itemName == "9x18mm" && item.amount > 1 ? item.amount.ToString() : "";
                text.fontSize = 28;
                text.alignment = TextAlignmentOptions.BottomRight;
            }

            string capturedId = item.itemId;
            Button btn = itemCell.GetComponent<Button>();
            btn.onClick.AddListener(() => {
                if (Input.GetKey(KeyCode.LeftShift))
                    MoveToBox(capturedId);
                else
                    UseItem(capturedId);
            });

            string tooltipName = item.itemName;
            EventTrigger trigger = itemCell.AddComponent<EventTrigger>();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => {
                if (tooltipText != null) tooltipText.gameObject.SetActive(true);
                if (tooltipText != null) tooltipText.text = tooltipName;
            });
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => {
                if (tooltipText != null) tooltipText.gameObject.SetActive(false);
            });
            trigger.triggers.Add(exitEntry);

            DraggableItem draggable = itemCell.AddComponent<DraggableItem>();
            draggable.itemName = item.itemId;
            draggable.fromInventory = true;
            draggable.inventory = this;
            draggable.boxContainer = boxContainer;
            draggable.dragGhost = dragGhostObject;
        }
    }
}
        
    
