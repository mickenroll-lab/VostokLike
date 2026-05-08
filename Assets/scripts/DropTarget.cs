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
        }
        // Box→インベントリ
        else if (!draggable.fromInventory && isInventory)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                int count = boxContainer.GetCount(draggable.itemName);
                boxContainer.MoveAllToPlayer(draggable.itemName);
            }
            else
            {
                inventory.AddItem(draggable.itemName);
                boxContainer.RemoveFromBox(draggable.itemName, 1);
            }
        }

        if (draggable.dragGhost != null)
            draggable.dragGhost.SetActive(false);

        Destroy(eventData.pointerDrag.gameObject);
    }
}