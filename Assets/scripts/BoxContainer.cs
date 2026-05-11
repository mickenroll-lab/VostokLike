using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ItemData;

public class BoxContainer : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;
    public GameObject gridCellPrefab;
    public Transform boxGridParent;

    public Inventory playerInventory;
    public EquipmentSlot weaponSlot;

    public PlayerState playerState;

    public GameObject boxGrid;
    public GameObject dragGhostObject;

    public int GetCount(string itemName)
    {
        if (boxContents.ContainsKey(itemName))
            return boxContents[itemName];
        return 0;
    }

    private int gridWidth = 8;
    private int gridHeight = 12;
    private Dictionary<string, int> boxContents = new Dictionary<string, int>();
    private ItemBox currentBox;
    private LootContainer currentLootContainer;
    private bool isOpen = false;

    public bool IsOpen { get { return isOpen; } }

    public void MoveAllToPlayer(string itemName)
    {
        if (!boxContents.ContainsKey(itemName)) return;
        int count = boxContents[itemName];
        for (int i = 0; i < count; i++)
            playerInventory.AddItem(itemName);
        boxContents.Remove(itemName);
        if (currentBox != null && currentBox.contents.ContainsKey(itemName))
            currentBox.contents.Remove(itemName);
        if (currentLootContainer != null)
            currentLootContainer.RemoveFromContents(itemName, count);
        UpdateBoxUI();
        playerInventory.UpdateInventoryUI();
    }

    public void OpenBox(Dictionary<string, int> contents, ItemBox box)
    {
        Debug.Log("BoxContainer.OpenBox(ItemBox) called\n" + System.Environment.StackTrace);
        currentBox = box;
        playerInventory.OpenFromBox();
        if (boxGrid != null) boxGrid.SetActive(true);
        boxContents = new Dictionary<string, int>(contents);
        isOpen = true;
        UpdateBoxUI();
    }

    public void OpenBox(Dictionary<string, int> contents)
    {
        Debug.Log("BoxContainer.OpenBox() called\n" + System.Environment.StackTrace);
        playerInventory.OpenFromBox();
        if (boxGrid != null) boxGrid.SetActive(true);
        boxContents = new Dictionary<string, int>(contents);
        isOpen = true;
        UpdateBoxUI();
    }

    public void CloseBox()
    {
        boxContents.Clear();
        UpdateBoxUI();
        if (boxGrid != null) boxGrid.SetActive(false);
        isOpen = false;
        currentLootContainer = null;
        if (playerInventory != null)
            playerInventory.CloseFromBox();
    }

    public void RemoveFromBox(string itemName, int amount)
    {
        if (!boxContents.ContainsKey(itemName)) return;
        boxContents[itemName] -= amount;
        if (boxContents[itemName] <= 0)
            boxContents.Remove(itemName);

        if (currentBox != null)
        {
            currentBox.contents[itemName] -= amount;
            if (currentBox.contents[itemName] <= 0)
                currentBox.contents.Remove(itemName);
        }
        if (currentLootContainer != null)
            currentLootContainer.RemoveFromContents(itemName, amount);
        UpdateBoxUI();
    }

    public void RemoveFromBox(string itemName)
    {
        if (boxContents.ContainsKey(itemName))
        {
            boxContents[itemName]--;
            if (boxContents[itemName] <= 0)
                boxContents.Remove(itemName);
        }
        if (currentBox != null)
        {
            if (currentBox.contents.ContainsKey(itemName))
            {
                currentBox.contents[itemName]--;
                if (currentBox.contents[itemName] <= 0)
                    currentBox.contents.Remove(itemName);
            }
        }
        if (currentLootContainer != null)
            currentLootContainer.RemoveFromContents(itemName, 1);
        UpdateBoxUI();
    }

    public void AddToBox(string itemName, int amount)
    {
        if (boxContents.ContainsKey(itemName))
            boxContents[itemName] += amount;
        else
            boxContents[itemName] = amount;
        if (currentBox != null)
        {
            if (currentBox.contents.ContainsKey(itemName))
                currentBox.contents[itemName] += amount;
            else
                currentBox.contents[itemName] = amount;
        }
        UpdateBoxUI();
    }

    public void OpenContainer(Dictionary<string, int> contents, int width, int height, LootContainer lootContainer = null)
    {
        Debug.Log($"BoxContainer.OpenContainer called contentsCount={contents.Count} width={width} height={height}");
        gridWidth = width;
        gridHeight = height;
        currentBox = null;
        currentLootContainer = lootContainer;
        if (playerInventory != null)
            playerInventory.OpenFromBox();
        if (boxGrid != null)
            boxGrid.SetActive(true);
        else
            Debug.LogWarning("BoxContainer.OpenContainer: boxGrid reference is null.");
        boxContents = new Dictionary<string, int>(contents);
        isOpen = true;
        UpdateBoxUI();
    }

    public void UpdateBoxUI()
    {
        if (boxGridParent == null)
        {
            Debug.LogWarning("BoxContainer.UpdateBoxUI: boxGridParent is null.");
            return;
        }
        foreach (Transform child in boxGridParent)
            Destroy(child.gameObject);

        int totalCells = gridWidth * gridHeight;
        int cellIndex = 0;
        List<string> keys = new List<string>(boxContents.Keys);

        foreach (string itemName in keys)
        {
            if (cellIndex >= totalCells) break;

            int count = boxContents[itemName];

            // Bullet（弾薬）のみスタック表示、Consumable・Weaponは個別セル
            bool isStackable = false;
            GameObject checkPrefab = Resources.Load<GameObject>(itemName);
            if (checkPrefab != null)
            {
                ItemData checkData = checkPrefab.GetComponent<ItemData>();
                if (checkData != null)
                    isStackable = checkData.category == ItemCategory.Bullet;
            }

            if (isStackable)
            {
                CreateCell(itemName, count, ref cellIndex);
            }
            else
            {
                for (int j = 0; j < count; j++)
                {
                    if (cellIndex >= totalCells) break;
                    CreateCell(itemName, 1, ref cellIndex);
                }
            }
        }

        while (cellIndex < totalCells)
        {
            GameObject cell = Instantiate(gridCellPrefab, boxGridParent);
            cell.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            cellIndex++;
        }
    }

    void PrimaryAction(string itemName)
    {
        GameObject prefab = Resources.Load<GameObject>(itemName);
        if (prefab == null) return;
        ItemData data = prefab.GetComponent<ItemData>();
        if (data == null) return;

        switch (data.category)
        {
            case ItemCategory.Consumable:
                RemoveFromBox(itemName, 1);
                playerInventory.ApplyConsumableEffect(itemName);
                break;

            case ItemCategory.Weapon:
                if (weaponSlot == null) return;
                weaponSlot.EquipFromBox(itemName, this);
                break;

            case ItemCategory.Bullet:
                Debug.Log("弾薬は直接使用不可: " + itemName);
                break;

            default:
                Debug.LogWarning("PrimaryAction未定義のカテゴリ: " + data.category);
                break;
        }
    }

    void CreateCell(string itemName, int displayCount, ref int cellIndex)
    {
        int itemW = 1;
        int itemH = 1;
        GameObject prefab = Resources.Load<GameObject>(itemName);
        if (prefab != null)
        {
            ItemData data = prefab.GetComponent<ItemData>();
            if (data != null)
            {
                itemW = data.gridWidth;
                itemH = data.gridHeight;
            }
        }

        for (int dy = 0; dy < itemH; dy++)
        {
            for (int dx = 0; dx < itemW; dx++)
            {
                if (cellIndex >= gridWidth * gridHeight) return;

                GameObject cell = Instantiate(gridCellPrefab, boxGridParent);
                Image cellImage = cell.GetComponent<Image>();
                cellImage.color = new Color(0.3f, 0.4f, 0.5f, 0.8f);

                if (dx == 0 && dy == 0)
                {
                    TextMeshProUGUI text = cell.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = displayCount > 1 ? displayCount.ToString() : "";
                        text.fontSize = 28;
                        text.alignment = TextAlignmentOptions.BottomRight;
                    }

                    string captured = itemName;
                    Button btn = cell.GetComponent<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                            MoveToPlayer(captured, true);
                        else
                            PrimaryAction(captured);
                    });

                    string tooltipName = itemName;
                    EventTrigger trigger = cell.AddComponent<EventTrigger>();

                    EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                    enterEntry.eventID = EventTriggerType.PointerEnter;
                    enterEntry.callback.AddListener((data) =>
                    {
                        if (tooltipText != null) tooltipText.gameObject.SetActive(true);
                        if (tooltipText != null) tooltipText.text = tooltipName;
                    });
                    trigger.triggers.Add(enterEntry);

                    EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                    exitEntry.eventID = EventTriggerType.PointerExit;
                    exitEntry.callback.AddListener((data) =>
                    {
                        if (tooltipText != null) tooltipText.gameObject.SetActive(false);
                    });
                    trigger.triggers.Add(exitEntry);

                    DraggableItem draggable = cell.AddComponent<DraggableItem>();
                    draggable.itemName = itemName;
                    draggable.fromInventory = false;
                    draggable.inventory = playerInventory;
                    draggable.boxContainer = this;
                    draggable.dragGhost = dragGhostObject;
                }
                cellIndex++;
            }

            void MoveToPlayer(string itemName, bool moveAll)
            {
                if (!boxContents.ContainsKey(itemName)) return;

                GameObject prefab = Resources.Load<GameObject>(itemName);
                ItemData data = prefab != null ? prefab.GetComponent<ItemData>() : null;

                int amount = 1;

                int itemW = data != null ? data.gridWidth : 1;
                int itemH = data != null ? data.gridHeight : 1;

                for (int i = 0; i < amount; i++)
                    playerInventory.AddItem(itemName, itemW, itemH);

                boxContents[itemName] -= amount;
                if (boxContents[itemName] <= 0)
                    boxContents.Remove(itemName);

                if (currentBox != null && currentBox.contents.ContainsKey(itemName))
                {
                    currentBox.contents[itemName] -= amount;
                    if (currentBox.contents[itemName] <= 0)
                        currentBox.contents.Remove(itemName);
                }

                if (currentLootContainer != null)
                    currentLootContainer.RemoveFromContents(itemName, amount);

                UpdateBoxUI();
                playerInventory.UpdateInventoryUI();
            }
        }
    }
}
