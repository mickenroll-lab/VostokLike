using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI hpText; 
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;

    public TextMeshProUGUI itemText;
    
    public TextMeshProUGUI interactText;
    private PlayerState playerState;

    void Start()
    {
        HUDManager[] managers = FindObjectsOfType<HUDManager>();
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
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
        }
        else
        {
            if (playerState.currentItem == "")
                itemText.text = "Empty";
            else
                itemText.text = "Item: " + playerState.currentItem;
        }

        hpText.text = " " + playerState.hp;
        staminaText.text = " " + (int)playerState.stamina;
        staminaText.color = playerState.stamina < 30f ? Color.red : Color.white;
        hungerText.text = " " + (int)playerState.hunger;
        hungerText.color = playerState.hunger < 30f ? Color.red : Color.white;
        thirstText.text = " " + (int)playerState.thirst;
        thirstText.color = playerState.thirst < 30f ? Color.red : Color.white;
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