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
                    // Box����
                    boxContainer.AddToBox(unequipItem, 1);
                }
                equipSlot.ForceUnequip();
                inventory.UpdateInventoryUI();
                if (!isInventory && boxContainer != null)
                    boxContainer.UpdateBoxUI();
                return;
            }
            return;
        }

        // �C���x���g����Box
        if (draggable.fromInventory && !isInventory)
        {
            inventory.MoveToBox(draggable.itemName);
            if (draggable.dragGhost != null) draggable.dragGhost.SetActive(false);
            Destroy(eventData.pointerDrag.gameObject);
        }
        // Box���C���x���g��
        else if (!draggable.fromInventory && isInventory)
        {
            // ItemData����T�C�Y���擾
            int itemW = 1;
            int itemH = 1;
            bool isStackable = false;
            GameObject prefab = Resources.Load<GameObject>(draggable.itemName);
            if (prefab != null)
            {
                ItemData data = prefab.GetComponent<ItemData>();
                if (data != null)
                {
                    itemW = data.gridWidth;
                    itemH = data.gridHeight;
                    isStackable = data.category == ItemData.ItemCategory.Bullet;
                }
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                boxContainer.MoveAllToPlayer(draggable.itemName);
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

