using UnityEngine;

public class Gun : MonoBehaviour
{
    public Inventory inventory; 
    public int maxAmmo = 6;
    private int currentAmmo;
    private bool isReloading = false;

    void Start()
    {
        currentAmmo = maxAmmo;
    }
    public GameObject hitEffectPrefab;
    void Update()
    {
        PlayerState state = GetComponentInParent<PlayerState>();
        if (state == null) return;
        if (state.currentItem != "Gun") return;

        Inventory inventory = GetComponentInParent<Inventory>();
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

    void Shoot()
    {
        currentAmmo--;
        Debug.Log("残弾：" + currentAmmo);

        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // エフェクト生成して2秒後に自動削除
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
                Destroy(effect, 0.1f);
            }

            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }
        }
    }

    System.Collections.IEnumerator Reload()
    {
        if (!inventory.HasAmmo("5.56x18mm"))
        {
            Debug.Log("Ammoがない！");
            yield break;
        }

        isReloading = true;
        Debug.Log("リロード中...");

        yield return new WaitForSeconds(2f);

        // リロードで消費する弾数を計算
        int needed = maxAmmo - currentAmmo;
        for (int i = 0; i < needed; i++)
        {
            if (!inventory.HasAmmo("5.56x18mm")) break;
            inventory.RemoveAmmo("5.56x18mm");
        }
        currentAmmo = maxAmmo;
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("リロード完了！");
    }
    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        currentAmmo = Mathf.Min(currentAmmo, maxAmmo);
        Debug.Log("弾薬追加：" + currentAmmo);
    }
}