using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ItemData;

public class EquipmentSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string slotType = "Weapon";
    public Inventory inventory;
    public PlayerState playerState;
    public Gun gun;
    public TextMeshProUGUI slotText;
    public BoxContainer boxContainer;

    private string equippedItem = "";
    
    public GameObject dragGhostObject;

    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable == null) return;

        // �@ fromInventory �`�F�b�N���폜�iBox�E�C���x���g����������󂯕t����j

        // �A �J�e�S���`�F�b�N�ǉ�
        GameObject prefab = Resources.Load<GameObject>(draggable.itemName);
        if (prefab == null) return;
        ItemData data = prefab.GetComponent<ItemData>();
        if (data == null) return;
        if (data.category != ItemCategory.Weapon)
        {
            Debug.Log("Weapon�J�e�S���ȊO�͑����s��: " + draggable.itemName);
            return;
        }

        // ���ɑ������̂��̂��C���x���g���ɖ߂�
        if (equippedItem != "")
        {
            GameObject equippedPrefab = Resources.Load<GameObject>(equippedItem);
            int w = 1, h = 1;
            if (equippedPrefab != null)
            {
                ItemData equippedData = equippedPrefab.GetComponent<ItemData>();
                if (equippedData != null) { w = equippedData.gridWidth; h = equippedData.gridHeight; }
            }
            inventory.AddItem(equippedItem, w, h);
            gun.Unequip();
        }

        equippedItem = draggable.itemName;
        playerState.currentItem = equippedItem;

        // �B �h���b�O���ɉ����ăA�C�e���폜���؂�ւ�
        if (draggable.fromInventory)
            inventory.RemoveItem(equippedItem);
        else
            draggable.boxContainer.RemoveFromBox(equippedItem); // Box������폜

        // WeaponData��T����Equip
        WeaponData weaponData = prefab.GetComponent<WeaponData>();
        if (weaponData != null)
            gun.Equip(weaponData);

        slotText.text = equippedItem;
        inventory.UpdateInventoryUI();

        // �C Box����̃h���b�O�Ȃ�BoxUI���X�V
        if (!draggable.fromInventory && draggable.boxContainer != null)
            draggable.boxContainer.UpdateBoxUI();

        Destroy(eventData.pointerDrag.gameObject);
        if (draggable.dragGhost != null)
            draggable.dragGhost.SetActive(false);
    }

    public void Unequip()
    {
        if (equippedItem == "") return;
        // Resources ���� ItemData ���擾���ăT�C�Y��n��
        GameObject prefab = Resources.Load<GameObject>(equippedItem);
        int w = 1, h = 1;
        if (prefab != null)
        {
            ItemData data = prefab.GetComponent<ItemData>();
            if (data != null) { w = data.gridWidth; h = data.gridHeight; }
        }
        inventory.AddItem(equippedItem, w, h);
        gun.Unequip();
        playerState.currentItem = "";
        equippedItem = "";
        slotText.text = "";
        inventory.UpdateInventoryUI();
    }
    public string GetEquippedItem()
    {
        return equippedItem;
    }

    public void ForceUnequip()
    {
        gun.Unequip();
        playerState.currentItem = "";
        equippedItem = "";
        slotText.text = "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (equippedItem == "") return;
        if (dragGhostObject == null) return;

        DraggableItem ghostDraggable = dragGhostObject.GetComponent<DraggableItem>();
        if (ghostDraggable == null)
            ghostDraggable = dragGhostObject.AddComponent<DraggableItem>();
        ghostDraggable.itemName = equippedItem;
        ghostDraggable.fromEquipmentSlot = this;
        ghostDraggable.dragGhost = dragGhostObject;
        ghostDraggable.fromInventory = false;
        ghostDraggable.boxContainer = boxContainer; // �� �ǉ�

        Image ghostImage = dragGhostObject.GetComponent<Image>();
        if (ghostImage != null)
            ghostImage.color = new Color(0.3f, 0.4f, 0.5f, 0.8f);
        TextMeshProUGUI ghostText = dragGhostObject.GetComponentInChildren<TextMeshProUGUI>();
        if (ghostText != null)
            ghostText.text = equippedItem;

        Debug.Log($"[EquipmentSlot] OnBeginDrag | equippedItem={equippedItem} | ghostDraggable={ghostDraggable} | fromEquipmentSlot={ghostDraggable.fromEquipmentSlot}");
        dragGhostObject.SetActive(true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhostObject != null)
            dragGhostObject.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragGhostObject != null)
            dragGhostObject.SetActive(false);
        canvasGroup.blocksRaycasts = true;
    }

    public void EquipFromInventory(string itemName)
    {
        GameObject prefab = Resources.Load<GameObject>(itemName);
        if (prefab == null) return;
        ItemData data = prefab.GetComponent<ItemData>();
        if (data == null) return;
        if (data.category != ItemCategory.Weapon) return;

        if (equippedItem != "")
        {
            GameObject equippedPrefab = Resources.Load<GameObject>(equippedItem);
            int w = 1, h = 1;
            if (equippedPrefab != null)
            {
                ItemData equippedData = equippedPrefab.GetComponent<ItemData>();
                if (equippedData != null) { w = equippedData.gridWidth; h = equippedData.gridHeight; }
            }
            inventory.AddItem(equippedItem, w, h);
            gun.Unequip();
        }

        equippedItem = itemName;
        playerState.currentItem = equippedItem;
        inventory.RemoveItem(itemName);

        WeaponData weaponData = prefab.GetComponent<WeaponData>();
        if (weaponData != null)
            gun.Equip(weaponData);

        slotText.text = equippedItem;
        inventory.UpdateInventoryUI();
    }

    public void EquipFromBox(string itemName, BoxContainer boxContainer)
    {
        // �J�e�S���`�F�b�N
        GameObject prefab = Resources.Load<GameObject>(itemName);
        if (prefab == null) return;
        ItemData data = prefab.GetComponent<ItemData>();
        if (data == null) return;
        if (data.category != ItemCategory.Weapon) return;

        // ���ɑ������̂��̂��C���x���g���ɖ߂�
        if (equippedItem != "")
        {
            GameObject equippedPrefab = Resources.Load<GameObject>(equippedItem);
            int w = 1, h = 1;
            if (equippedPrefab != null)
            {
                ItemData equippedData = equippedPrefab.GetComponent<ItemData>();
                if (equippedData != null) { w = equippedData.gridWidth; h = equippedData.gridHeight; }
            }
            inventory.AddItem(equippedItem, w, h);
            gun.Unequip();
        }

        // ��������
        equippedItem = itemName;
        playerState.currentItem = equippedItem;
        boxContainer.RemoveFromBox(itemName, 1);

        WeaponData weaponData = prefab.GetComponent<WeaponData>();
        if (weaponData != null)
            gun.Equip(weaponData);

        slotText.text = equippedItem;
        inventory.UpdateInventoryUI();
        boxContainer.UpdateBoxUI();
    }
}