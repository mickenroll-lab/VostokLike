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
            // ItemDataからサイズを取得
            int itemW = 1;
            int itemH = 1;
            GameObject prefab = Resources.Load<GameObject>(draggable.itemName);
            if (prefab != null)
            {
                ItemData data = prefab.GetComponent<ItemData>();
                if (data != null)
                {
                    itemW = data.gridWidth;
                    itemH = data.gridHeight;
                }
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                boxContainer.MoveAllToPlayer(draggable.itemName);
            }
            else
            {
                inventory.AddItem(draggable.itemName, itemW, itemH);
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

