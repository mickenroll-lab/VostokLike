using System.Collections.Generic;
using UnityEngine;

public class StorageContainer : MonoBehaviour
{
    public BoxContainer boxContainer;

    public Dictionary<string, int> contents = new Dictionary<string, int>();

    public void Interact()
    {
        if (boxContainer != null && boxContainer.IsOpen)
        {
            boxContainer.CloseBox();
            return;
        }
        if (boxContainer == null)
        {
            Debug.LogWarning("StorageContainer.Interact: boxContainer is null.");
            return;
        }
        boxContainer.OpenStorage(contents, 4, 4, this);
    }

    public void RemoveFromContents(string itemName, int amount)
    {
        if (!contents.ContainsKey(itemName)) return;
        contents[itemName] -= amount;
        if (contents[itemName] <= 0)
            contents.Remove(itemName);
    }

    public void AddToContents(string itemName, int amount)
    {
        if (contents.ContainsKey(itemName))
            contents[itemName] += amount;
        else
            contents[itemName] = amount;
    }
}
