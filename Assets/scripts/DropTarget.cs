using UnityEngine;
using UnityEngine.EventSystems;

public class DropTarget : MonoBehaviour, IDropHandler
{
    public bool isInventory;
    public Inventory inventory;
    public BoxContainer boxContainer;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop呼ばれた");
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable == null) return;

        // インベントリ→Box
        if (draggable.fromInventory && !isInventory)
        {
            inventory.MoveToBox(draggable.itemName);
            if (draggable.dragGhost != null) draggable.dragGhost.SetActive(false);
            Destroy(eventData.pointerDrag.gameObject);
        }
        // Box→インベントリ
        else if (!draggable.fromInventory && isInventory)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                boxContainer.MoveAllToPlayer(draggable.itemName);
            }
            else
            {
                inventory.AddItem(draggable.itemName);
                boxContainer.RemoveFromBox(draggable.itemName, 1);
            }
            inventory.UpdateInventoryUI();
            boxContainer.UpdateBoxUI();
            if (draggable.dragGhost != null) draggable.dragGhost.SetActive(false);
            Destroy(eventData.pointerDrag.gameObject);
        }
        // 同じグリッド内へのドロップは何もしない
        else
        {
            if (draggable.dragGhost != null) draggable.dragGhost.SetActive(false);
            inventory.UpdateInventoryUI();
        }
    }
}

