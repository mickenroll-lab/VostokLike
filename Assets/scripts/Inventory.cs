using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    public BoxContainer boxContainer;

    public void MoveToBoxItem(InventoryItem specificItem)
    {
        if (boxContainer == null || !boxGrid.activeSelf) return;
        if (specificItem == null || !inventoryItems.Contains(specificItem)) return;

        GameObject prefab = Resources.Load<GameObject>(specificItem.itemName);
        ItemData data = prefab?.GetComponent<ItemData>();
        if (data != null && data.category == ItemData.ItemCategory.Magazine)
            boxContainer.AddMagazineToBox(specificItem.itemName, specificItem.ammo);
        else if (data != null && data.category == ItemData.ItemCategory.Weapon)
            boxContainer.AddWeaponToBox(specificItem.itemName, specificItem.ammo);
        else
            boxContainer.AddToBox(specificItem.itemName, specificItem.amount);

        if (items.ContainsKey(specificItem.itemName))
        {
            items[specificItem.itemName] -= specificItem.amount;
            if (items[specificItem.itemName] <= 0)
                items.Remove(specificItem.itemName);
        }

        inventoryGrid.RemoveItem(specificItem.itemId);
        inventoryItems.Remove(specificItem);

        UpdateInventoryUI();
        boxContainer.UpdateBoxUI();
    }

    public void MoveToBox(string itemName, bool moveAll = false)
    {
        if (boxContainer == null || !boxGrid.activeSelf) return;

        GameObject prefab = Resources.Load<GameObject>(itemName);
        ItemData data = prefab?.GetComponent<ItemData>();
        bool isMagazine = data != null && data.category == ItemData.ItemCategory.Magazine;
        bool isWeapon = data != null && data.category == ItemData.ItemCategory.Weapon;

        if (moveAll)
        {
            int count = items.ContainsKey(itemName) ? items[itemName] : 0;
            if (count <= 0) return;

            List<InventoryItem> toRemove = inventoryItems.FindAll(i => i.itemName == itemName);
            if (isMagazine)
            {
                foreach (InventoryItem mag in toRemove)
                    boxContainer.AddMagazineToBox(mag.itemName, mag.ammo);
            }
            else if (isWeapon)
            {
                foreach (InventoryItem wpn in toRemove)
                    boxContainer.AddWeaponToBox(wpn.itemName, wpn.ammo);
            }
            else
                boxContainer.AddToBox(itemName, count);

            items.Remove(itemName);
            foreach (InventoryItem inv in toRemove)
            {
                inventoryGrid.RemoveItem(inv.itemId);
                inventoryItems.Remove(inv);
            }
        }
        else
        {
            InventoryItem target = inventoryItems.Find(i => i.itemName == itemName);
            int amount = target != null ? target.amount : 1;
            if (isMagazine && target != null)
                boxContainer.AddMagazineToBox(itemName, target.ammo);
            else if (isWeapon && target != null)
                boxContainer.AddWeaponToBox(itemName, target.ammo);
            else
                boxContainer.AddToBox(itemName, amount);
            if (items.ContainsKey(itemName))
            {
                items[itemName] -= amount;
                if (items[itemName] <= 0)
                    items.Remove(itemName);
            }
            if (target != null)
            {
                inventoryGrid.RemoveItem(target.itemId);
                inventoryItems.Remove(target);
            }
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
    public EquipmentSlot equipmentSlot;

    public GameObject crosshair;
    public GameObject dragGhostObject;

    public bool isOpen = false;
    private bool justOpened = false;

    private InventoryGrid inventoryGrid;
    private List<InventoryItem> inventoryItems = new List<InventoryItem>();

    private int gridWidth = 10;
    private int gridHeight = 12;

    // アイテム名と数量を管理
    public Dictionary<string, int> items = new Dictionary<string, int>();

    public bool justClosed = false;
    private InventoryItem hoveredItem = null;

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

        if (justOpened)
        {
            justOpened = false;
            return;
        }

        if (SleepManager.IsSleeping) return;

        if (Input.GetKeyDown(KeyCode.F) && isOpen)
        {
            if (boxContainer != null && boxContainer.IsOpen)
            {
                boxContainer.CloseBox(); // isOpen含め全て CloseFromBox() 経由でリセット
            }
            else
            {
                isOpen = false;
                justClosed = true;
                inventoryPanel.SetActive(false);
                if (crosshair != null) crosshair.SetActive(true);
                tooltipText.gameObject.SetActive(false);
                if (boxGrid != null) boxGrid.SetActive(false);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);
            if (crosshair != null) crosshair.SetActive(!isOpen);
            tooltipText.gameObject.SetActive(false);
            if (boxGrid != null) boxGrid.SetActive(false);
            if (!isOpen && boxContainer != null && boxContainer.IsOpen)
                boxContainer.CloseBox();
            if (isOpen)
                UpdateInventoryUI();
        }

    }

    void DropItem(InventoryItem item)
    {
        if (item == null || !inventoryItems.Contains(item)) return;

        Transform cam = Camera.main.transform;
        Vector3 spawnPos = cam.position + cam.forward * 1.5f;

        GameObject dropped = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dropped.transform.position = spawnPos;
        dropped.transform.localScale = Vector3.one * 0.3f;

        DroppedItem di = dropped.AddComponent<DroppedItem>();
        di.itemName = item.itemName;
        di.amount = item.amount;

        Rigidbody rb = dropped.AddComponent<Rigidbody>();
        rb.AddForce(cam.forward * 3f + Vector3.up * 1f, ForceMode.Impulse);

        if (items.ContainsKey(item.itemName))
        {
            items[item.itemName] -= item.amount;
            if (items[item.itemName] <= 0)
                items.Remove(item.itemName);
        }

        inventoryGrid.RemoveItem(item.itemId);
        inventoryItems.Remove(item);

        hoveredItem = null;
        UpdateInventoryUI();
    }

    public void OpenFromBox()
    {
        isOpen = true;
        justOpened = true;
        inventoryPanel.SetActive(true);
        if (crosshair != null) crosshair.SetActive(false);
        Debug.Log("Inventory.OpenFromBox: opened inventory from box");
        UpdateInventoryUI();
    }

    public void CloseFromBox()
    {
        isOpen = false;
        justClosed = true;
        inventoryPanel.SetActive(false);
        if (crosshair != null) crosshair.SetActive(true);
        tooltipText.gameObject.SetActive(false);
        if (boxGrid != null) boxGrid.SetActive(false);
    }

    public void AddItem(string itemName)
    {
        int stackLimit = GetStackLimit(itemName);

        if (items.ContainsKey(itemName))
            items[itemName]++;
        else
            items[itemName] = 1;

        // stackLimit > 1（弾薬等）のみ既存セルにスタック、それ以外は常に新規セル
        if (stackLimit > 1)
        {
            InventoryItem existing = inventoryItems.Find(item => item.itemName == itemName);
            if (existing != null)
            {
                existing.amount++;
                Debug.Log("アイテム追加（スタック）：" + itemName);
                return;
            }
        }

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
        int stackLimit = GetStackLimit(itemName);

        if (items.ContainsKey(itemName))
            items[itemName]++;
        else
            items[itemName] = 1;

        // stackLimit > 1（弾薬等）のみ既存セルにスタック、それ以外は常に新規セル
        if (stackLimit > 1)
        {
            InventoryItem existing = inventoryItems.Find(item => item.itemName == itemName);
            if (existing != null)
            {
                existing.amount++;
                UpdateInventoryUI();
                return;
            }
        }

        int foundX, foundY;
        if (inventoryGrid.FindFreeSpace(w, h, out foundX, out foundY))
        {
            InventoryItem newItem = new InventoryItem(itemName, foundX, foundY, w, h);
            inventoryItems.Add(newItem);
            inventoryGrid.PlaceItem(newItem.itemId, foundX, foundY, w, h);
        }
        UpdateInventoryUI();
    }

    public void AddItem(string itemName, int w, int h, int ammo)
    {
        int stackLimit = GetStackLimit(itemName);

        if (items.ContainsKey(itemName))
            items[itemName]++;
        else
            items[itemName] = 1;

        if (stackLimit > 1)
        {
            InventoryItem existing = inventoryItems.Find(item => item.itemName == itemName);
            if (existing != null)
            {
                existing.amount++;
                UpdateInventoryUI();
                return;
            }
        }

        int foundX, foundY;
        if (inventoryGrid.FindFreeSpace(w, h, out foundX, out foundY))
        {
            InventoryItem newItem = new InventoryItem(itemName, foundX, foundY, w, h);
            newItem.ammo = ammo;
            inventoryItems.Add(newItem);
            inventoryGrid.PlaceItem(newItem.itemId, foundX, foundY, w, h);
        }
        UpdateInventoryUI();
    }

    public int GetAmmo(string itemName)
    {
        InventoryItem found = inventoryItems.Find(i => i.itemName == itemName);
        return found != null ? found.ammo : 0;
    }

    public int GetItemCount(string itemName)
    {
        return items.ContainsKey(itemName) ? items[itemName] : 0;
    }

    // UpdateInventoryUI を呼ばずに弾薬をまとめて削除する（D&D中のDestroyImmediate回避用）
    public void RemoveAmmoBatch(string itemName, int count)
    {
        if (!items.ContainsKey(itemName) || count <= 0) return;
        int actual = Mathf.Min(count, items[itemName]);
        items[itemName] -= actual;
        InventoryItem target = inventoryItems.Find(i => i.itemName == itemName);
        if (target != null)
        {
            target.amount -= actual;
            if (items[itemName] <= 0)
            {
                items.Remove(itemName);
                inventoryGrid.RemoveItem(target.itemId);
                inventoryItems.Remove(target);
            }
        }
    }

    // D&D中にUpdateInventoryUIを直接呼ぶとDestroyImmediateがイベント処理中のオブジェクトを壊すため1フレーム遅延させる
    public void ScheduleUIUpdate()
    {
        StartCoroutine(UpdateUINextFrame());
    }

    System.Collections.IEnumerator UpdateUINextFrame()
    {
        yield return null;
        UpdateInventoryUI();
    }

    // ammo > 0 のマガジンを残弾数降順で返す
    public InventoryItem FindMagazine(string magazineName)
    {
        return inventoryItems
            .FindAll(i => i.itemName == magazineName && i.ammo > 0)
            .OrderByDescending(i => i.ammo)
            .FirstOrDefault();
    }

    public bool HasFreeSpace(int w, int h)
    {
        int x, y;
        return inventoryGrid.FindFreeSpace(w, h, out x, out y);
    }

    public bool ContainsItem(InventoryItem item) => inventoryItems.Contains(item);

    // 特定のInventoryItemをインベントリから直接削除する
    public void RemoveItemDirectly(InventoryItem item)
    {
        if (!inventoryItems.Contains(item)) return;
        if (items.ContainsKey(item.itemName))
        {
            items[item.itemName]--;
            if (items[item.itemName] <= 0)
                items.Remove(item.itemName);
        }
        inventoryGrid.RemoveItem(item.itemId);
        inventoryItems.Remove(item);
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
            Debug.Log("回復した");
            InventoryItem target = inventoryItems.Find(item => item.itemName == itemName);
            if (target != null)
            {
                if (target.amount > 1)
                    target.amount--;
                else
                {
                    inventoryGrid.RemoveItem(target.itemId);
                    inventoryItems.Remove(target);
                }
            }
            UpdateInventoryUI(); // ← 最後に移動
        }


        else if (itemName == "BeefCan")
        {
            itemManager.PickupFood(30);
            items[itemName]--;
            if (items[itemName] <= 0)
                items.Remove(itemName);
            InventoryItem target = inventoryItems.Find(item => item.itemName == itemName);
            if (target != null)
            {
                if (target.amount > 1)
                    target.amount--;
                else
                {
                    inventoryGrid.RemoveItem(target.itemId);
                    inventoryItems.Remove(target);
                }
            }
            UpdateInventoryUI();
        }
        else if (itemName == "Water")
        {
            itemManager.PickupWater(30);
            items[itemName]--;
            if (items[itemName] <= 0)
                items.Remove(itemName);
            InventoryItem target = inventoryItems.Find(item => item.itemName == itemName);
            if (target != null)
            {
                if (target.amount > 1)
                    target.amount--;
                else
                {
                    inventoryGrid.RemoveItem(target.itemId);
                    inventoryItems.Remove(target);
                }
            }
            UpdateInventoryUI();
        }
    }


    public void ApplyConsumableEffect(string itemName)
    {
        if (itemName == "MedKit")
            itemManager.PickupMedKit(30);
        else if (itemName == "BeefCan")
            itemManager.PickupFood(30);
        else if (itemName == "Water")
            itemManager.PickupWater(30);
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

        // ↓ 追加：グリッドからも削除
        InventoryItem target = inventoryItems.Find(item => item.itemName == itemName);
        if (target != null)
        {
            inventoryGrid.RemoveItem(target.itemId);
            inventoryItems.Remove(target);
        }

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
            InventoryItem target = inventoryItems.Find(item => item.itemName == itemName);
            if (target != null)
            {
                target.amount--;
                if (items[itemName] <= 0)
                {
                    items.Remove(itemName);
                    inventoryGrid.RemoveItem(target.itemId);
                    inventoryItems.Remove(target);
                }
            }
        }
        else if (items.ContainsKey(itemName + "_2") && items[itemName + "_2"] > 0)
        {
            items[itemName + "_2"]--;
            InventoryItem target = inventoryItems.Find(item => item.itemName == itemName);
            if (target != null)
            {
                target.amount--;
                if (items[itemName + "_2"] <= 0)
                {
                    items.Remove(itemName + "_2");
                    inventoryGrid.RemoveItem(target.itemId);
                    inventoryItems.Remove(target);
                }
            }
        }
        UpdateInventoryUI();
    }

    public void ClearInventory()
    {
        items.Clear();
        inventoryItems.Clear();
        inventoryGrid.Clear();
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        // 既存処理
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
                            draggable.inventoryItem = item;
                            draggable.fromInventory = true;
                            draggable.inventory = this;
                            draggable.boxContainer = boxContainer;
                            draggable.dragGhost = dragGhostObject;
                        }
                    {
                        TextMeshProUGUI text = itemCell.GetComponentInChildren<TextMeshProUGUI>();
                        if (text != null)
                        {
                            text.text = item.amount > 1 ? item.amount.ToString() : "";
                            text.fontSize = 30;
                            text.color = Color.white;
                            text.alignment = TextAlignmentOptions.BottomRight;
                        }

                        string captured = item.itemName;
                        InventoryItem capturedItem = item;
                        itemCell.GetComponent<Button>().onClick.AddListener(() => {
                            if (Input.GetKey(KeyCode.LeftControl))
                                DropItem(capturedItem);
                            else if (Input.GetKey(KeyCode.LeftShift))
                                MoveToBoxItem(capturedItem);
                            else
                            {
                                GameObject p = Resources.Load<GameObject>(captured);
                                ItemData d = p != null ? p.GetComponent<ItemData>() : null;
                                if (d != null && d.category == ItemData.ItemCategory.Weapon && equipmentSlot != null)
                                    equipmentSlot.EquipFromInventory(capturedItem);
                                else if (d != null && d.category == ItemData.ItemCategory.Magazine && equipmentSlot != null)
                                    equipmentSlot.gun?.ReloadWith(capturedItem);
                                else
                                    UseItem(captured);
                            }
                        });

                        string tooltipName = item.itemName;
                        EventTrigger trigger = itemCell.AddComponent<EventTrigger>();

                        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                        enterEntry.eventID = EventTriggerType.PointerEnter;
                        enterEntry.callback.AddListener((data) => {
                            tooltipText.gameObject.SetActive(true);
                            tooltipText.text = tooltipName;
                            hoveredItem = capturedItem;
                        });
                        trigger.triggers.Add(enterEntry);

                        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                        exitEntry.eventID = EventTriggerType.PointerExit;
                        exitEntry.callback.AddListener((data) => {
                            tooltipText.gameObject.SetActive(false);
                            hoveredItem = null;
                        });
                        trigger.triggers.Add(exitEntry);

                        DraggableItem draggable = itemCell.AddComponent<DraggableItem>();
                        draggable.itemName = item.itemName;
                        draggable.inventoryItem = item;
                        draggable.fromInventory = true;
                        draggable.inventory = this;
                        draggable.boxContainer = boxContainer;
                        draggable.dragGhost = dragGhostObject;

                        // 武器・マガジンセルの残弾数表示＋右クリックアクション
                        if (dx == 0 && dy == 0)
                        {
                            GameObject wPrefab = Resources.Load<GameObject>(item.itemName);
                            ItemData wData = wPrefab?.GetComponent<ItemData>();
                            if (wData != null && wData.category == ItemData.ItemCategory.Weapon)
                            {
                                AmmoDisplay ammoDisplay = itemCell.AddComponent<AmmoDisplay>();
                                WeaponData wd = wPrefab?.GetComponent<WeaponData>();
                                if (wd != null)
                                    ammoDisplay.Show(item.ammo, wd.magazineSize);
                            }
                            else if (wData != null && wData.category == ItemData.ItemCategory.Magazine)
                            {
                                AmmoDisplay ammoDisplay = itemCell.AddComponent<AmmoDisplay>();
                                ammoDisplay.Show(item.ammo, wData.maxAmmo);

                                MagazineDropHandler dropHandler = itemCell.AddComponent<MagazineDropHandler>();
                                dropHandler.inventory = this;
                                dropHandler.magazineItem = item;
                                dropHandler.magazineData = wData;
                            }

                            // 右クリック：武器＝マガジン取り出し、マガジン＝アンロード
                            if (wData != null && (wData.category == ItemData.ItemCategory.Weapon || wData.category == ItemData.ItemCategory.Magazine))
                            {
                                ItemData capturedWData = wData;
                                EventTrigger.Entry rightClickEntry = new EventTrigger.Entry();
                                rightClickEntry.eventID = EventTriggerType.PointerClick;
                                rightClickEntry.callback.AddListener((eventData) =>
                                {
                                    PointerEventData ped = (PointerEventData)eventData;
                                    if (ped.button != PointerEventData.InputButton.Right) return;
                                    if (capturedWData.category == ItemData.ItemCategory.Weapon)
                                    {
                                        if (equipmentSlot != null)
                                            equipmentSlot.gun?.RemoveMagazineToInventory();
                                    }
                                    else if (capturedWData.category == ItemData.ItemCategory.Magazine)
                                    {
                                        if (capturedItem.ammo > 0)
                                        {
                                            for (int i = 0; i < capturedItem.ammo; i++)
                                                AddItem(capturedWData.compatibleBullet);
                                            capturedItem.ammo = 0;
                                            ScheduleUIUpdate();
                                        }
                                    }
                                });
                                trigger.triggers.Add(rightClickEntry);
                            }
                        }
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