using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public int ammoCount = 0;
    private PlayerState playerState;

    void Start()
    {
        playerState = GetComponent<PlayerState>();
    }

    public void PickupAmmo(int amount)
    {
        Gun gun = GetComponentInChildren<Gun>();
        if (gun != null)
            gun.AddAmmo(amount);
        else
            Debug.Log("Gun‚ªŒ©‚Â‚©‚ç‚È‚¢");
    }

    public void PickupMedKit(int amount)
    {
        if (playerState == null) return;
        playerState.hp += amount;
        playerState.hp = Mathf.Min(playerState.hp, 100);
        Debug.Log("HP‰ñ•œF" + playerState.hp);
    }
}