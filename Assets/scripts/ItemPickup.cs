using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public BoxContainer boxContainer;
    public float pickupRange = 3f;
    public Camera playerCamera;
    public Transform hand;
    public HUDManager hudManager;
    public Inventory inventory;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }
    void Update()
    {
        if (playerCamera == null) return;

        if (inventory != null && inventory.inventoryPanel.activeSelf) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.CompareTag("Item") || hit.collider.CompareTag("Exit") || hit.collider.CompareTag("ItemBox"))
                hudManager.ShowInteractText(hit.collider.gameObject.name);
            else
                hudManager.HideInteractText();
        }
        else
        {
            hudManager.HideInteractText();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (inventory != null && (inventory.isOpen || inventory.justClosed)) return;

            if (Physics.Raycast(ray, out hit, pickupRange))
            {
                if (hit.collider.CompareTag("Item"))
                {
                    GameObject item = hit.collider.gameObject;
                    ItemData data = item.GetComponent<ItemData>();
                    PlayerState state = GetComponent<PlayerState>();

                    if (data == null) return;

                    if (data.category == ItemData.ItemCategory.Weapon)
                    {
                        item.transform.SetParent(hand);
                        item.transform.localPosition = Vector3.zero;
                        item.transform.localRotation = Quaternion.identity;
                        item.GetComponent<Collider>().enabled = false;
                        state.currentItem = data.itemType;
                        Debug.Log("装備：" + state.currentItem);
                    }
                    else if (data.category == ItemData.ItemCategory.Consumable)
                    {
                        inventory.AddItem(data.itemType);
                        Destroy(item);
                    }
                }
                else if (hit.collider.CompareTag("ItemBox"))
                {
                    ItemBox box = hit.collider.GetComponent<ItemBox>();
                    if (box != null && box.contents.Count > 0)
                    {
                        boxContainer.OpenBox(box.contents, box);
                        Debug.Log("アイテムボックスを開けた");
                    }
                }
            }
        }
    }
}