using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public string itemName;
    public int amount = 1;
    private Inventory inventory;

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    public void Pickup()
    {
        if (inventory == null) return;
        GameObject prefab = Resources.Load<GameObject>(itemName);
        int w = 1, h = 1;
        if (prefab != null)
        {
            ItemData data = prefab.GetComponent<ItemData>();
            if (data != null) { w = data.gridWidth; h = data.gridHeight; }
        }
        for (int i = 0; i < amount; i++)
            inventory.AddItem(itemName, w, h);
        Destroy(gameObject);
    }
}
