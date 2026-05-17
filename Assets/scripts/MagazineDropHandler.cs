using UnityEngine;
using UnityEngine.EventSystems;

// マガジンセルにアタッチ。9x18mmをドラッグ&ドロップすると満タンまで一括装填する。
public class MagazineDropHandler : MonoBehaviour, IDropHandler
{
    public Inventory inventory;
    public InventoryItem magazineItem;
    public ItemData magazineData;
    public BoxContainer boxContainer; // BoxGridのマガジンセルの場合にセット

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (draggable == null) return;

        GameObject prefab = Resources.Load<GameObject>(draggable.itemName);
        ItemData data = prefab?.GetComponent<ItemData>();
        if (data == null || data.category != ItemData.ItemCategory.Bullet) return;
        if (draggable.itemName != magazineData.compatibleBullet) return;

        int needed = magazineData.maxAmmo - magazineItem.ammo;
        if (needed <= 0) return;

        int available = draggable.fromInventory
            ? inventory.GetItemCount(draggable.itemName)
            : (draggable.boxContainer?.GetCount(draggable.itemName) ?? 0);

        int toLoad = Mathf.Min(needed, available);
        if (toLoad <= 0) return;

        if (draggable.fromInventory)
        {
            inventory.RemoveAmmoBatch(draggable.itemName, toLoad);
            inventory.ScheduleUIUpdate();
        }
        else
        {
            draggable.boxContainer?.RemoveFromBox(draggable.itemName, toLoad);
            draggable.boxContainer?.UpdateBoxUI();
        }

        magazineItem.ammo += toLoad;
        Debug.Log($"[MagazineDropHandler] {draggable.itemName}×{toLoad}装填 → {magazineItem.ammo}/{magazineData.maxAmmo}");

        boxContainer?.UpdateBoxUI();

        if (draggable.dragGhost != null) draggable.dragGhost.SetActive(false);
    }
}
