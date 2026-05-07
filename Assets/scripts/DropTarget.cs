using UnityEngine;
using UnityEngine.EventSystems;

public class DropTarget : MonoBehaviour, IDropHandler
{
    public bool isInventory;
    public Inventory inventory;
    public BoxContainer boxContainer;


    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable == null) return;

        // インベントリ→Box
        if (draggable.fromInventory && !isInventory)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                int count = inventory.items.ContainsKey(draggable.itemName) ? inventory.items[draggable.itemName] : 0;
                boxContainer.AddToBox(draggable.itemName, count);
                inventory.items.Remove(draggable.itemName);
            }
            else
            {
                boxContainer.AddToBox(draggable.itemName, 1);
                inventory.RemoveItem(draggable.itemName);
            }
            inventory.UpdateInventoryUI();
            boxContainer.UpdateBoxUI();
        }
        // Box→インベントリ
        else if (!draggable.fromInventory && isInventory)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                int count = boxContainer.GetCount(draggable.itemName);
                for (int i = 0; i < count; i++)
                    inventory.AddItem(draggable.itemName);
                boxContainer.RemoveFromBox(draggable.itemName, count);
            }
            else
            {
                inventory.AddItem(draggable.itemName);
                boxContainer.RemoveFromBox(draggable.itemName, 1);
            }
            inventory.UpdateInventoryUI();
            boxContainer.UpdateBoxUI();
        }

        // ゴーストを非表示
        if (draggable.dragGhost != null)
            draggable.dragGhost.SetActive(false);
    }
}