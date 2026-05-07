using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI itemText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI interactText;
    private PlayerState playerState;

    void Start()
    {
        playerState = FindObjectOfType<PlayerState>();
        
        interactText.text = "";
    }

    void Update()
    {
        if (playerState == null) return;

        // インベントリが開いていたらInteractTextを非表示
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null && inventory.inventoryPanel.activeSelf)
        {
            interactText.text = "";
            return;
        }

        if (playerState.currentItem == "")
            itemText.text = "Empty";
        else
            itemText.text = "Item: " + playerState.currentItem;

        hpText.text = "HP: " + playerState.hp;
    }

    public void ShowInteractText(string name)
    {
        
        interactText.text = "[F] " + name;
    }

    public void HideInteractText()
    {
        interactText.text = "";
    }
}