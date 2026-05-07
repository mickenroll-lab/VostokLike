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
    private int gridWidth = 10;
    private int gridHeight = 12;

    // アイテム名と数量を管理
    public Dictionary<string, int> items = new Dictionary<string, int>();

    public bool justClosed = false;

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

        Debug.Log("アイテム追加：" + itemName);
    }

    int GetStackLimit(string itemName)
    {
        if (itemName == "5.56x18mm") return 30;
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
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        int totalCells = gridWidth * gridHeight;
        int itemIndex = 0;

        List<string> itemKeys = new List<string>(items.Keys);

        for (int i = 0; i < totalCells; i++)
        {
            GameObject cell = Instantiate(gridCellPrefab, gridParent);
            Image cellImage = cell.GetComponent<Image>();

            if (itemIndex < itemKeys.Count)
            {
                string itemName = itemKeys[itemIndex];
                int count = items[itemName];

                cellImage.color = new Color(0.3f, 0.5f, 0.3f, 0.8f);
                TextMeshProUGUI text = cell.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = count > 1 ? count.ToString() : "";
                    text.fontSize = 28;
                    text.alignment = TextAlignmentOptions.BottomRight;
                }

                string captured = itemName;
                cell.GetComponent<Button>().onClick.AddListener(() => {
                    if (Input.GetKey(KeyCode.LeftShift))
                        MoveToBox(captured, true);
                    else
                        UseItem(captured);
                });
                // DraggableItemをアタッチ
                DraggableItem draggable = cell.AddComponent<DraggableItem>();
                draggable.itemName = itemName;
                draggable.fromInventory = true;
                draggable.inventory = this;
                draggable.boxContainer = boxContainer;
                draggable.dragGhost = dragGhostObject; // ← これが入っているか
                itemIndex++;
                // マウスオーバーでツールチップ表示
                string tooltipName = itemName;
                EventTrigger trigger = cell.AddComponent<EventTrigger>();

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
            }
            else
            {
                cellImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            }
        }
    }
}