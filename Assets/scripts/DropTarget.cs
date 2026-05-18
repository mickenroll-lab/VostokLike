using UnityEngine;
using UnityEngine.EventSystems;

public class DropTarget : MonoBehaviour, IDropHandler
{
    public bool isInventory;
    public Inventory inventory;
    public BoxContainer boxContainer;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop�Ă΂ꂽ");
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();

        // WeaponSlot����̃h���b�O�̓S�[�X�g���̂�DraggableItem������
        if (draggable == null)
        {
            EquipmentSlot equipSlot = eventData.pointerDrag.GetComponent<EquipmentSlot>();
            if (equipSlot != null)
            {
                string unequipItem = equipSlot.GetEquippedItem();
                if (unequipItem == "") return;
                int itemW = 1, itemH = 1;
                GameObject prefab = Resources.Load<GameObject>(unequipItem);
                if (prefab != null)
                {
                    ItemData data = prefab.GetComponent<ItemData>();
                    if (data != null) { itemW = data.gridWidth; itemH = data.gridHeight; }
                }
                if (isInventory)
                {
                    inventory.AddItem(unequipItem, itemW, itemH, equipSlot.GetSavedAmmo());
                }
                else
                {
                    boxContainer.AddWeaponToBox(unequipItem, equipSlot.GetSavedAmmo());
                }
                equipSlot.ForceUnequip();
                inventory.UpdateInventoryUI();
                if (!isInventory && boxContainer != null)
                    boxContainer.UpdateBoxUI();
                return;
            }
            return;
        }

        // インベントリ→Box
        if (draggable.fromInventory && !isInventory)
        {
            if (draggable.inventoryItem != null)
                inventory.MoveToBoxItem(draggable.inventoryItem);
            else
                inventory.MoveToBox(draggable.itemName);
            if (draggable.dragGhost != null) draggable.dragGhost.SetActive(false);
            Destroy(eventData.pointerDrag.gameObject);
        }
        // Box→インベントリ
        else if (!draggable.fromInventory && isInventory)
        {
            int itemW = 1;
            int itemH = 1;
            bool isStackable = false;
            bool isMagazine = false;
            bool isWeapon = false;
            ItemData itemData = null;
            GameObject prefab = Resources.Load<GameObject>(draggable.itemName);
            if (prefab != null)
            {
                itemData = prefab.GetComponent<ItemData>();
                if (itemData != null)
                {
                    itemW = itemData.gridWidth;
                    itemH = itemData.gridHeight;
                    isStackable = itemData.category == ItemData.ItemCategory.Bullet;
                    isMagazine = itemData.category == ItemData.ItemCategory.Magazine;
                    isWeapon = itemData.category == ItemData.ItemCategory.Weapon;
                }
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                boxContainer.MoveAllToPlayer(draggable.itemName);
            }
            else if (isMagazine)
            {
                if (draggable.inventoryItem != null)
                {
                    inventory.AddItem(draggable.itemName, itemW, itemH, draggable.inventoryItem.ammo);
                    boxContainer.RemoveInventoryItemFromBox(draggable.inventoryItem);
                }
                else
                {
                    int ammo = boxContainer.GetFirstMagazineAmmo(draggable.itemName);
                    inventory.AddItem(draggable.itemName, itemW, itemH, ammo);
                    boxContainer.RemoveFromBox(draggable.itemName, 1);
                }
            }
            else if (isWeapon)
            {
                if (draggable.inventoryItem != null)
                {
                    inventory.AddItem(draggable.itemName, itemW, itemH, draggable.inventoryItem.ammo);
                    boxContainer.RemoveInventoryItemFromBox(draggable.inventoryItem);
                }
                else
                {
                    int ammo = boxContainer.GetFirstMagazineAmmo(draggable.itemName);
                    inventory.AddItem(draggable.itemName, itemW, itemH, ammo);
                    boxContainer.RemoveFromBox(draggable.itemName, 1);
                }
            }
            else
            {
                int transferAmount = isStackable ? boxContainer.GetCount(draggable.itemName) : 1;
                for (int i = 0; i < transferAmount; i++)
                    inventory.AddItem(draggable.itemName, itemW, itemH);
                boxContainer.RemoveFromBox(draggable.itemName, transferAmount);
            }
            inventory.UpdateInventoryUI();
            boxContainer.UpdateBoxUI();
            if (draggable.dragGhost != null) draggable.dragGhost.SetActive(false);
            Destroy(eventData.pointerDrag.gameObject);
        }
        // �����O���b�h���ւ̃h���b�v�͉������Ȃ�
        else
        {
            if (draggable.dragGhost != null) draggable.dragGhost.SetActive(false);
            inventory.UpdateInventoryUI();
        }
    }
}

