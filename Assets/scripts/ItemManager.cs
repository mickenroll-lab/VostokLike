using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public int ammoCount = 0;
    private PlayerState playerState;

    void Start()
    {
        playerState = GetComponent<PlayerState>();
    }

  

    public void PickupMedKit(int amount)
    {
        if (playerState == null) return;
        playerState.hp += amount;
        playerState.hp = Mathf.Min(playerState.hp, 100);
        Debug.Log("HP�񕜁F" + playerState.hp);
    }
    public void PickupFood(int amount)
    {
        PlayerState playerState = GetComponent<PlayerState>();
        if (playerState == null) return;
        playerState.hunger += amount;
        playerState.hunger = Mathf.Min(playerState.hunger, playerState.hungerMax);
        playerState.HealOverTime(10);
        Debug.Log("�H���񕜁F" + playerState.hunger);
    }

    public void PickupWater(int amount)
    {
        PlayerState playerState = GetComponent<PlayerState>();
        if (playerState == null) return;
        playerState.thirst += amount;
        playerState.thirst = Mathf.Min(playerState.thirst, playerState.thirstMax);
        Debug.Log("�����񕜁F" + playerState.thirst);
    }
}