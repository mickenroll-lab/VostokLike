using UnityEngine;
using UnityEngine.EventSystems;

// マガジンセルにアタッチ。9x18mmをドラッグ&ドロップすると満タンまで一括装填する。
public class MagazineDropHandler : MonoBehaviour, IDropHandler
{
    public Inventory inventory;
    public InventoryItem magazineItem;
    public ItemData magazineData;

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

        int available = inventory.GetItemCount(draggable.itemName);
        int toLoad = Mathf.Min(needed, available);
        if (toLoad <= 0) return;

        // RemoveAmmoBatch: UpdateInventoryUI を呼ばないバッチ削除
        inventory.RemoveAmmoBatch(draggable.itemName, toLoad);
        magazineItem.ammo += toLoad;
        Debug.Log($"[MagazineDropHandler] {draggable.itemName}×{toLoad}装填 → {magazineItem.ammo}/{magazineData.maxAmmo}");

        // 1フレーム遅延でUI更新（D&D中のDestroyImmediate回避）
        inventory.ScheduleUIUpdate();
    }
}
