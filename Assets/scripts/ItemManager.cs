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
        Debug.Log("HP回復：" + playerState.hp);
    }
    public void PickupFood(int amount)
    {
        PlayerState playerState = GetComponent<PlayerState>();
        if (playerState == null) return;
        playerState.hunger += amount;
        playerState.hunger = Mathf.Min(playerState.hunger, playerState.hungerMax);
        Debug.Log("食料回復：" + playerState.hunger);
    }

    public void PickupWater(int amount)
    {
        PlayerState playerState = GetComponent<PlayerState>();
        if (playerState == null) return;
        playerState.thirst += amount;
        playerState.thirst = Mathf.Min(playerState.thirst, playerState.thirstMax);
        Debug.Log("水分回復：" + playerState.thirst);
    }
}