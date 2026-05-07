using UnityEngine;

public class WeaponData : MonoBehaviour
{
    [Header("Šî–{î•ñ")]
    public string weaponName;
    public string weaponType; // Pistol, Rifle, Shotgun ‚È‚Ç

    [Header("’e–ò")]
    public string ammoType; // 9x18mm ‚È‚Ç
    public int magazineSize;
    public int currentAmmo;

    [Header("«”\")]
    public float damage;
    public float range;
    public float fireRate;
    public float reloadTime;
}