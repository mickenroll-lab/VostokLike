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
        int itemIndex = 0;
        List<string> keys = new List<string>(boxContents.Keys);

        for (int i = 0; i < totalCells; i++)
        {
            GameObject cell = Instantiate(gridCellPrefab, boxGridParent);
            Image cellImage = cell.GetComponent<Image>();

            if (itemIndex < keys.Count)
            {
                string itemName = keys[itemIndex];
                int count = boxContents[itemName];

                cellImage.color = new Color(0.3f, 0.4f, 0.5f, 0.8f);
                TextMeshProUGUI text = cell.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = count > 1 ? count.ToString() : "";
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

                // DraggableItemをアタッチ
                DraggableItem draggable = cell.AddComponent<DraggableItem>();
                draggable.itemName = itemName;
                draggable.fromInventory = false;
                draggable.inventory = playerInventory;
                draggable.boxContainer = this;
                draggable.dragGhost = dragGhostObject; // ← 追加

                itemIndex++;
            }
            else
            {
                cellImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            }
        }
    }

    void MoveToPlayer(string itemName, bool moveAll)
    {
        int amount = moveAll ? boxContents[itemName] : 1;

        for (int i = 0; i < amount; i++)
            playerInventory.AddItem(itemName);

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