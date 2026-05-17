using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;

    public TextMeshProUGUI itemText;
    public TextMeshProUGUI ammoText;

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
        if (ammoText != null) ammoText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerState == null) return;

        // �C���x���g�����J���Ă�����InteractText���\��
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
        hpText.color = StatusColor(playerState.hp, playerState.hpMax);
        staminaText.text = " " + (int)playerState.stamina;
        staminaText.color = StatusColor(playerState.stamina, playerState.staminaMax);
        hungerText.text = " " + (int)playerState.hunger;
        hungerText.color = StatusColor(playerState.hunger, playerState.hungerMax);
        thirstText.text = " " + (int)playerState.thirst;
        thirstText.color = StatusColor(playerState.thirst, playerState.thirstMax);
    }

    static readonly Color ColorGreen = new Color(0x44 / 255f, 0xFF / 255f, 0x2F / 255f);

    Color StatusColor(float value, float max)
    {
        float pct = max > 0 ? value / max * 100f : 0f;
        if (pct >= 80f) return ColorGreen;
        if (pct <= 20f) return Color.red;
        return Color.white;
    }

    public void ShowInteractText(string name)
    {
        interactText.text = "[F] " + name;
    }

    public void HideInteractText()
    {
        interactText.text = "";
    }

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText == null) return;
        ammoText.gameObject.SetActive(true);
        ammoText.text = current + " / " + max;
        ammoText.color = current == 0 ? Color.red : Color.white;
    }

    public void HideAmmo()
    {
        if (ammoText == null) return;
        ammoText.gameObject.SetActive(false);
    }
}