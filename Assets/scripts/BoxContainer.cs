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
    private List<InventoryItem> boxInventoryItems = new List<InventoryItem>();
    private ItemBox currentBox;
    private LootContainer currentLootContainer;
    private StorageContainer currentStorageContainer;
    private bool isOpen = false;

    public bool IsOpen { get { return isOpen; } }

    // boxContentsにあるマガジンをboxInventoryItemsにmaxAmmoで初期化（Open時のみ使用）
    void InitializeMagazineAmmo()
    {
        foreach (string itemName in boxContents.Keys)
        {
            GameObject prefab = Resources.Load<GameObject>(itemName);
            ItemData data = prefab?.GetComponent<ItemData>();
            if (data == null || data.category != ItemCategory.Magazine) continue;
            for (int i = 0; i < boxContents[itemName]; i++)
            {
                InventoryItem magItem = new InventoryItem(itemName, 0, 0, data.gridWidth, data.gridHeight);
                magItem.ammo = data.maxAmmo;
                boxInventoryItems.Add(magItem);
            }
        }
    }

    public int GetFirstMagazineAmmo(string itemName)
    {
        InventoryItem found = boxInventoryItems.Find(i => i.itemName == itemName);
        return found?.ammo ?? 0;
    }

    public void MoveAllToPlayer(string itemName)
    {
        if (!boxContents.ContainsKey(itemName)) return;
        int count = boxContents[itemName];

        GameObject prefab = Resources.Load<GameObject>(itemName);
        ItemData data = prefab?.GetComponent<ItemData>();
        if (data != null && data.category == ItemCategory.Magazine)
        {
            int w = data.gridWidth;
            int h = data.gridHeight;
            List<InventoryItem> magItems = new List<InventoryItem>(boxInventoryItems.FindAll(i => i.itemName == itemName));
            for (int i = 0; i < count; i++)
            {
                int ammo = i < magItems.Count ? magItems[i].ammo : data.maxAmmo;
                playerInventory.AddItem(itemName, w, h, ammo);
            }
            boxInventoryItems.RemoveAll(i => i.itemName == itemName);
        }
        else
        {
            for (int i = 0; i < count; i++)
                playerInventory.AddItem(itemName);
        }

        boxContents.Remove(itemName);
        if (currentBox != null && currentBox.contents.ContainsKey(itemName))
            currentBox.contents.Remove(itemName);
        if (currentLootContainer != null)
            currentLootContainer.RemoveFromContents(itemName, count);
        if (currentStorageContainer != null)
            currentStorageContainer.RemoveFromContents(itemName, count);
        UpdateBoxUI();
        playerInventory.UpdateInventoryUI();
    }

    public void OpenBox(Dictionary<string, int> contents, ItemBox box)
    {
        currentBox = box;
        currentLootContainer = null;
        currentStorageContainer = null;
        playerInventory.OpenFromBox();
        if (boxGrid != null) boxGrid.SetActive(true);
        boxContents = new Dictionary<string, int>(contents);
        boxInventoryItems.Clear();
        InitializeMagazineAmmo();
        isOpen = true;
        UpdateBoxUI();
    }

    public void OpenBox(Dictionary<string, int> contents)
    {
        currentBox = null;
        currentLootContainer = null;
        currentStorageContainer = null;
        playerInventory.OpenFromBox();
        if (boxGrid != null) boxGrid.SetActive(true);
        boxContents = new Dictionary<string, int>(contents);
        boxInventoryItems.Clear();
        InitializeMagazineAmmo();
        isOpen = true;
        UpdateBoxUI();
    }

    public void CloseBox()
    {
        boxContents.Clear();
        boxInventoryItems.Clear();
        UpdateBoxUI();
        if (boxGrid != null) boxGrid.SetActive(false);
        isOpen = false;
        currentLootContainer = null;
        currentStorageContainer = null;
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
        if (currentStorageContainer != null)
            currentStorageContainer.RemoveFromContents(itemName, amount);

        for (int i = 0; i < amount; i++)
        {
            InventoryItem boxMag = boxInventoryItems.Find(j => j.itemName == itemName);
            if (boxMag != null) boxInventoryItems.Remove(boxMag);
        }
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
        if (currentStorageContainer != null)
            currentStorageContainer.RemoveFromContents(itemName, 1);

        InventoryItem boxMag = boxInventoryItems.Find(i => i.itemName == itemName);
        if (boxMag != null) boxInventoryItems.Remove(boxMag);
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
        if (currentStorageContainer != null)
            currentStorageContainer.AddToContents(itemName, amount);
        UpdateBoxUI();
    }

    public void AddMagazineToBox(string itemName, int ammo)
    {
        if (boxContents.ContainsKey(itemName))
            boxContents[itemName]++;
        else
            boxContents[itemName] = 1;
        if (currentBox != null)
        {
            if (currentBox.contents.ContainsKey(itemName))
                currentBox.contents[itemName]++;
            else
                currentBox.contents[itemName] = 1;
        }
        if (currentStorageContainer != null)
            currentStorageContainer.AddToContents(itemName, 1);
        InventoryItem magItem = new InventoryItem(itemName, 0, 0, 1, 2);
        magItem.ammo = ammo;
        boxInventoryItems.Add(magItem);
        UpdateBoxUI();
    }

    public void OpenContainer(Dictionary<string, int> contents, int width, int height, LootContainer lootContainer = null)
    {
        Debug.Log($"BoxContainer.OpenContainer called contentsCount={contents.Count} width={width} height={height}");
        gridWidth = width;
        gridHeight = height;
        currentBox = null;
        currentLootContainer = lootContainer;
        currentStorageContainer = null;
        if (playerInventory != null)
            playerInventory.OpenFromBox();
        if (boxGrid != null)
            boxGrid.SetActive(true);
        else
            Debug.LogWarning("BoxContainer.OpenContainer: boxGrid reference is null.");
        boxContents = new Dictionary<string, int>(contents);
        boxInventoryItems.Clear();
        InitializeMagazineAmmo();
        isOpen = true;
        UpdateBoxUI();
    }

    public void OpenStorage(Dictionary<string, int> contents, int width, int height, StorageContainer storageContainer)
    {
        gridWidth = width;
        gridHeight = height;
        currentBox = null;
        currentLootContainer = null;
        currentStorageContainer = storageContainer;
        if (playerInventory != null)
            playerInventory.OpenFromBox();
        if (boxGrid != null)
            boxGrid.SetActive(true);
        else
            Debug.LogWarning("BoxContainer.OpenStorage: boxGrid reference is null.");
        boxContents = new Dictionary<string, int>(contents);
        boxInventoryItems.Clear();
        InitializeMagazineAmmo();
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
                CreateCell(itemName, count, ref cellIndex, null);
            }
            else
            {
                List<InventoryItem> magItems = boxInventoryItems.FindAll(i => i.itemName == itemName);
                for (int j = 0; j < count; j++)
                {
                    if (cellIndex >= totalCells) break;
                    InventoryItem sourceItem = j < magItems.Count ? magItems[j] : null;
                    CreateCell(itemName, 1, ref cellIndex, sourceItem);
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

    void MoveOneItemToPlayer(string itemName, InventoryItem sourceItem = null)
    {
        if (!boxContents.ContainsKey(itemName)) return;
        GameObject prefab = Resources.Load<GameObject>(itemName);
        ItemData data = prefab?.GetComponent<ItemData>();
        int w = data?.gridWidth ?? 1;
        int h = data?.gridHeight ?? 1;

        int ammo = 0;
        InventoryItem boxMag = sourceItem ?? boxInventoryItems.Find(i => i.itemName == itemName);
        if (boxMag != null)
        {
            ammo = boxMag.ammo;
            boxInventoryItems.Remove(boxMag);
        }
        playerInventory.AddItem(itemName, w, h, ammo);

        boxContents[itemName]--;
        if (boxContents[itemName] <= 0) boxContents.Remove(itemName);
        if (currentBox != null && currentBox.contents.ContainsKey(itemName))
        {
            currentBox.contents[itemName]--;
            if (currentBox.contents[itemName] <= 0) currentBox.contents.Remove(itemName);
        }
        if (currentLootContainer != null) currentLootContainer.RemoveFromContents(itemName, 1);
        if (currentStorageContainer != null) currentStorageContainer.RemoveFromContents(itemName, 1);
        UpdateBoxUI();
        playerInventory.UpdateInventoryUI();
    }

    void PrimaryAction(string itemName)
    {
        if (tooltipText != null) tooltipText.gameObject.SetActive(false);

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

            case ItemCategory.Magazine:
                MoveOneItemToPlayer(itemName);
                break;

            case ItemCategory.Bullet:
                Debug.Log("弾薬は直接使用不可: " + itemName);
                break;

            default:
                Debug.LogWarning("PrimaryAction未定義のカテゴリ: " + data.category);
                break;
        }
    }

    void CreateCell(string itemName, int displayCount, ref int cellIndex, InventoryItem sourceItem)
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

                // メインセルのみカウントテキストを表示
                if (dx == 0 && dy == 0)
                {
                    TextMeshProUGUI text = cell.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = displayCount > 1 ? displayCount.ToString() : "";
                        text.fontSize = 30;
                        text.color = Color.white;
                        text.alignment = TextAlignmentOptions.BottomRight;
                    }
                }

                // 全セルにButton・EventTrigger・DraggableItemをアタッチ
                string captured = itemName;
                InventoryItem capturedSource = sourceItem;
                Button btn = cell.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        MoveToPlayer(captured, true);
                    }
                    else
                    {
                        GameObject p = Resources.Load<GameObject>(captured);
                        ItemData d = p?.GetComponent<ItemData>();
                        if (d != null && d.category == ItemCategory.Magazine && capturedSource != null && weaponSlot?.gun != null)
                        {
                            // BoxGridのマガジンをインベントリに移してリロード開始
                            MoveOneItemToPlayer(captured, capturedSource);
                            InventoryItem newMag = weaponSlot.gun.inventory.FindMagazine(captured);
                            if (newMag != null)
                                weaponSlot.gun.ReloadWith(newMag);
                        }
                        else
                            PrimaryAction(captured);
                    }
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

                // 武器・マガジンセルの残弾数表示＋右クリックアクション（メインセルのみ）
                if (dx == 0 && dy == 0)
                {
                    GameObject wPrefab = Resources.Load<GameObject>(itemName);
                    ItemData wData = wPrefab?.GetComponent<ItemData>();

                    if (wData != null && wData.category == ItemCategory.Weapon)
                    {
                        AmmoDisplay ammoDisplay = cell.AddComponent<AmmoDisplay>();
                        string equipped = weaponSlot?.GetEquippedItem() ?? "";
                        Gun gun = weaponSlot?.gun;
                        if (equipped == itemName && gun != null)
                            ammoDisplay.Show(gun.GetCurrentAmmo(), gun.GetMagazineSize());

                        // 右クリック：マガジン取り出し
                        EventTrigger.Entry rightClickEntry = new EventTrigger.Entry();
                        rightClickEntry.eventID = EventTriggerType.PointerClick;
                        rightClickEntry.callback.AddListener((eventData) =>
                        {
                            PointerEventData ped = (PointerEventData)eventData;
                            if (ped.button != PointerEventData.InputButton.Right) return;
                            weaponSlot?.gun?.RemoveMagazineToInventory();
                        });
                        trigger.triggers.Add(rightClickEntry);
                    }
                    else if (wData != null && wData.category == ItemCategory.Magazine && capturedSource != null)
                    {
                        AmmoDisplay ammoDisplay = cell.AddComponent<AmmoDisplay>();
                        ammoDisplay.Show(capturedSource.ammo, wData.maxAmmo);

                        MagazineDropHandler dropHandler = cell.AddComponent<MagazineDropHandler>();
                        dropHandler.inventory = playerInventory;
                        dropHandler.magazineItem = capturedSource;
                        dropHandler.magazineData = wData;
                        dropHandler.boxContainer = this;

                        // 右クリック：アンロード
                        ItemData capturedWData = wData;
                        EventTrigger.Entry rightClickEntry = new EventTrigger.Entry();
                        rightClickEntry.eventID = EventTriggerType.PointerClick;
                        rightClickEntry.callback.AddListener((eventData) =>
                        {
                            PointerEventData ped = (PointerEventData)eventData;
                            if (ped.button != PointerEventData.InputButton.Right) return;
                            if (capturedSource.ammo > 0)
                            {
                                for (int i = 0; i < capturedSource.ammo; i++)
                                    playerInventory.AddItem(capturedWData.compatibleBullet);
                                capturedSource.ammo = 0;
                                UpdateBoxUI();
                            }
                        });
                        trigger.triggers.Add(rightClickEntry);
                    }
                }

                cellIndex++;
            }

            void MoveToPlayer(string itemName, bool moveAll)
            {
                if (!boxContents.ContainsKey(itemName)) return;

                GameObject prefab = Resources.Load<GameObject>(itemName);
                ItemData data = prefab != null ? prefab.GetComponent<ItemData>() : null;

                int amount = displayCount;
                int itemW = data != null ? data.gridWidth : 1;
                int itemH = data != null ? data.gridHeight : 1;

                // マガジンはammoを引き継いでMoveOneItemToPlayerへ
                if (data != null && data.category == ItemCategory.Magazine)
                {
                    MoveOneItemToPlayer(itemName, sourceItem);
                    return;
                }

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
                if (currentStorageContainer != null)
                    currentStorageContainer.RemoveFromContents(itemName, amount);

                UpdateBoxUI();
                playerInventory.UpdateInventoryUI();
            }
        }
    }
}
