using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class EquipmentSlot : MonoBehaviour, IDropHandler
{
    public string slotType = "Weapon";
    public Inventory inventory;
    public PlayerState playerState;
    public Gun gun;
    public TextMeshProUGUI slotText;

    private string equippedItem = "";

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable == null) return;
        if (!draggable.fromInventory) return;

        // インベントリのアイテムがWeaponか確認
        // 既に装備中のものをインベントリに戻す
        if (equippedItem != "")
        {
            inventory.AddItem(equippedItem);
            gun.Unequip();
        }

        equippedItem = draggable.itemName;
        playerState.currentItem = equippedItem;
        inventory.RemoveItem(equippedItem);

        // WeaponDataを探してEquip
        GameObject weaponPrefab = Resources.Load<GameObject>(equippedItem);
        if (weaponPrefab != null)
        {
            WeaponData weaponData = weaponPrefab.GetComponent<WeaponData>();
            if (weaponData != null)
                gun.Equip(weaponData);
        }

        slotText.text = equippedItem;
        inventory.UpdateInventoryUI();

        Destroy(eventData.pointerDrag.gameObject);
        if (draggable.dragGhost != null)
            draggable.dragGhost.SetActive(false);
    }

    public void Unequip()
    {
        if (equippedItem == "") return;
        inventory.AddItem(equippedItem);
        gun.Unequip();
        playerState.currentItem = "";
        equippedItem = "";
        slotText.text = "";
        inventory.UpdateInventoryUI();
    }
}