using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoxContainer : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;
    public GameObject gridCellPrefab;
    public Transform boxGridParent;
    public Inventory playerInventory;


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


    public void MoveAllToPlayer(string itemName)
    {
        if (!boxContents.ContainsKey(itemName)) return;
        int count = boxContents[itemName];
        for (int i = 0; i < count; i++)
            playerInventory.AddItem(itemName);
        boxContents.Remove(itemName);
        if (currentBox != null && currentBox.contents.ContainsKey(itemName))
            currentBox.contents.Remove(itemName);
        UpdateBoxUI();
        playerInventory.UpdateInventoryUI();
    }
    public void OpenBox(Dictionary<string, int> contents, ItemBox box)
    {
        currentBox = box;
        playerInventory.OpenFromBox();
        if (boxGrid != null) boxGrid.SetActive(true);
        boxContents = new Dictionary<string, int>(contents);
        UpdateBoxUI();
    }

    public void OpenBox(Dictionary<string, int> contents)
    {
        playerInventory.OpenFromBox();
        if (boxGrid != null) boxGrid.SetActive(true);
        boxContents = new Dictionary<string, int>(contents);
        UpdateBoxUI();
    }

    public void CloseBox()
    {
        boxContents.Clear();
        UpdateBoxUI();
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
        UpdateBoxUI();
    }
    public void UpdateBoxUI()
    {
        foreach (Transform child in boxGridParent)
            Destroy(child.gameObject);

        int totalCells = gridWidth * gridHeight;
        int cellIndex = 0;
        List<string> keys = new List<string>(boxContents.Keys);

        foreach (string itemName in keys)
        {
            if (cellIndex >= totalCells) break;

            int count = boxContents[itemName];
            bool isAmmo = itemName == "9x18mm";

            if (isAmmo)
            {
                // 弾薬はスタック表示（1セル）
                CreateCell(itemName, count, true, ref cellIndex);
            }
            else
            {
                // それ以外は個別表示
                for (int j = 0; j < count; j++)
                {
                    if (cellIndex >= totalCells) break;
                    CreateCell(itemName, 1, false, ref cellIndex);
                }
            }
        }

        // 残りの空セルを埋める
        while (cellIndex < totalCells)
        {
            GameObject cell = Instantiate(gridCellPrefab, boxGridParent);
            cell.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            cellIndex++;
        }
    }

    void CreateCell(string itemName, int displayCount, bool isAmmo, ref int cellIndex)
    {
        // ItemDataからサイズを取得
        int itemW = 1;
        int itemH = 1;
        if (!isAmmo)
        {
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
        }

        // 複数セルにわたって色を付ける
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
                        text.text = isAmmo && displayCount > 1 ? displayCount.ToString() : "";
                        text.fontSize = 28;
                        text.alignment = TextAlignmentOptions.BottomRight;
                    }

                    string captured = itemName;
                    Button btn = cell.GetComponent<Button>();
                    btn.onClick.AddListener(() => MoveToPlayer(captured, Input.GetKey(KeyCode.LeftShift)));

                    string tooltipName = itemName;
                    EventTrigger trigger = cell.AddComponent<EventTrigger>();

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

                   
                }
                else
                {
                    string captured = itemName;
                    Button btn = cell.GetComponent<Button>();
                    btn.onClick.AddListener(() => MoveToPlayer(captured, Input.GetKey(KeyCode.LeftShift)));

                    // ToolTip追加
                    string tooltipName = itemName;
                    EventTrigger trigger = cell.AddComponent<EventTrigger>();

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

                    DraggableItem draggable = cell.AddComponent<DraggableItem>();
                    draggable.itemName = itemName;
                    draggable.fromInventory = false;
                    draggable.inventory = playerInventory;
                    draggable.boxContainer = this;
                    draggable.dragGhost = dragGhostObject;
                }
                cellIndex++;
            }
        }
    }
    void MoveToPlayer(string itemName, bool moveAll)
    {
        bool isAmmo = itemName == "9x18mm";
        int amount = (moveAll && isAmmo) ? boxContents[itemName] : 1;

        // ItemDataからサイズを取得
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

        for (int i = 0; i < amount; i++)
            playerInventory.AddItem(itemName, itemW, itemH);

        boxContents[itemName] -= amount;
        if (boxContents[itemName] <= 0)
            boxContents.Remove(itemName);

        if (currentBox != null)
        {
            currentBox.contents[itemName] -= amount;
            if (currentBox.contents[itemName] <= 0)
                currentBox.contents.Remove(itemName);
        }

        UpdateBoxUI();
        playerInventory.UpdateInventoryUI();
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
}