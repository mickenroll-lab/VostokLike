using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public Inventory inventory;
    public GameObject hitEffectPrefab;

    private WeaponData currentWeapon;
    private int currentAmmo;
    private bool isReloading = false;

    void Update()
    {
        PlayerState state = GetComponent<PlayerState>();
        if (state == null) return;
        if (state.currentItem == "") return;

        // インベントリが開いていたら射撃不可
        if (inventory != null && inventory.inventoryPanel.activeSelf) return;
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentAmmo <= 0)
            {
                Debug.Log("弾がない！");
                return;
            }
            Shoot();
        }
    }

    public void Equip(WeaponData weapon)
    {
        currentWeapon = weapon;
        currentAmmo = weapon.magazineSize;
        Debug.Log("装備：" + weapon.weaponName);
    }

    public void Unequip()
    {
        currentWeapon = null;
        currentAmmo = 0;
    }

    void Shoot()
    {
        if (currentWeapon == null) return;

        currentAmmo--;
        Debug.Log("残弾：" + currentAmmo);

        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, currentWeapon.range))
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
                Destroy(effect, 0.1f);
            }

            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage((int)currentWeapon.damage);
        }
    }

    IEnumerator Reload()
    {
        if (currentWeapon == null) yield break;
        if (!inventory.HasAmmo(currentWeapon.ammoType))
        {
            Debug.Log("Ammoがない！");
            yield break;
        }

        isReloading = true;
        Debug.Log("リロード中...");
        yield return new WaitForSeconds(currentWeapon.reloadTime);

        int needed = currentWeapon.magazineSize - currentAmmo;
        for (int i = 0; i < needed; i++)
        {
            if (!inventory.HasAmmo(currentWeapon.ammoType)) break;
            inventory.RemoveAmmo(currentWeapon.ammoType);
        }

        currentAmmo = currentWeapon.magazineSize;
        isReloading = false;
        Debug.Log("リロード完了！");
    }
}