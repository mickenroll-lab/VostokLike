using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public Inventory inventory;
    public HUDManager hudManager;
    public GameObject hitEffectPrefab;

    public ParticleSystem muzzleFlash;
    public ParticleSystem smokeEffect;
    public AudioClip gunShotSound;

    private WeaponData currentWeapon;
    private int currentAmmo;
    private bool isReloading = false;
    private bool magazineLoaded = false;

    private Camera mainCamera;
    private Vector3 originalCameraPos;
    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.05f;
    private AudioSource audioSource;



    void Start()
    {
        mainCamera = Camera.main;
        originalCameraPos = mainCamera.transform.localPosition;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        ConfigureParticles();
    }

    void ConfigureParticles()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = muzzleFlash.main;
            main.duration = 0.05f;
            main.loop = false;
            main.startLifetime = 0.05f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(1f, 0.95f, 0.7f));
            main.maxParticles = 30;

            var emission = muzzleFlash.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

            var shape = muzzleFlash.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.01f;

            Shader glowShader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (glowShader == null) glowShader = Shader.Find("Particles/Additive");
            if (glowShader != null)
                muzzleFlash.GetComponent<Renderer>().material = new Material(glowShader);
        }

        if (smokeEffect != null)
        {
            smokeEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = smokeEffect.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            main.startColor = new Color(0.7f, 0.7f, 0.7f, 0.4f);
            main.gravityModifier = -0.05f;
            main.maxParticles = 15;

            var emission = smokeEffect.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 5) });

            var shape = smokeEffect.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.02f;

            var col = smokeEffect.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.7f, 0.7f, 0.7f), 0f),
                    new GradientColorKey(new Color(0.7f, 0.7f, 0.7f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.4f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = grad;
        }
    }

    void Update()
    {
        PlayerState state = GetComponent<PlayerState>();
        if (state == null) return;

        if (Input.GetKeyDown(KeyCode.R))
            Debug.Log("[Gun] R検出(ガード前) currentItem='" + state.currentItem + "' inventoryOpen=" + (inventory != null && inventory.inventoryPanel.activeSelf) + " isReloading=" + isReloading);

        if (state.currentItem == "") return;

        if (inventory != null && inventory.inventoryPanel.activeSelf) return;
        if (state.deathPanel != null && state.deathPanel.activeSelf) return;
        if (RaidManager.Instance != null && RaidManager.Instance.resultManager != null && RaidManager.Instance.resultManager.resultPanel.activeSelf) return;

        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[Gun] R押下(ガード通過) currentAmmo=" + currentAmmo + " isReloading=" + isReloading);
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (ItemGrabber.Instance != null && ItemGrabber.Instance.IsHolding) return;
            if (!magazineLoaded)
            {
                Debug.Log("マガジンがない！Rキーでリロード");
                return;
            }
            if (currentAmmo <= 0)
            {
                Debug.Log("弾がない！Rキーでリロード");
                return;
            }
            Shoot();
        }
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMagazineSize() => currentWeapon != null ? currentWeapon.magazineSize : 0;

    public void Equip(WeaponData weapon, int initialAmmo = 0)
    {
        StopAllCoroutines();
        isReloading = false;
        currentWeapon = weapon;
        currentAmmo = initialAmmo;
        magazineLoaded = initialAmmo > 0;
        Debug.Log($"[Gun] Equip: {weapon.weaponName} currentAmmo={currentAmmo} magazineLoaded={magazineLoaded}");
        hudManager?.UpdateAmmo(currentAmmo, currentWeapon.magazineSize);
    }

    public void Unequip()
    {
        StopAllCoroutines();
        isReloading = false;
        currentWeapon = null;
        currentAmmo = 0;
        magazineLoaded = false;
        hudManager?.HideAmmo();
    }

    void Shoot()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();
        if (smokeEffect != null)
            smokeEffect.Play();
        if (audioSource != null && gunShotSound != null)
            audioSource.PlayOneShot(gunShotSound);

        if (currentWeapon == null) return;

        currentAmmo--;
        if (currentAmmo <= 0) magazineLoaded = false;
        Debug.Log("残弾：" + currentAmmo);
        hudManager?.UpdateAmmo(currentAmmo, currentWeapon.magazineSize);

        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        PlayerState ps = GetComponent<PlayerState>();
        if (ps != null && ps.mental < 40f)
            ray = new Ray(ray.origin, (ray.direction + Random.insideUnitSphere * 0.05f).normalized);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, currentWeapon.range))
        {
            Debug.Log("Raycastヒット: " + hit.collider.name + " tag: " + hit.collider.tag);
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
                Destroy(effect, 0.1f);
            }

            Enemy enemy = hit.collider.GetComponent<Enemy>() ?? hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage((int)currentWeapon.damage);
        }
        StartCoroutine(CameraShake());
    }
    IEnumerator CameraShake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            mainCamera.transform.localPosition = originalCameraPos + Random.insideUnitSphere * shakeMagnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.localPosition = originalCameraPos;
    }

    IEnumerator Reload()
    {
        if (currentWeapon == null) { Debug.Log("[Reload] 中断：武器なし"); yield break; }

        string magName = currentWeapon.weaponName + "Magazine";
        InventoryItem newMag = inventory.FindMagazine(magName);
        if (newMag == null)
        {
            Debug.Log($"[Reload] 中断：{magName}がインベントリにない");
            yield break;
        }

        isReloading = true;
        Debug.Log($"[Reload] 開始 currentAmmo={currentAmmo} newMag.ammo={newMag.ammo}");
        yield return new WaitForSeconds(currentWeapon.reloadTime);

        if (currentWeapon == null) { isReloading = false; yield break; }

        // 旧マガジンをインベントリに戻す（残弾があれば）
        if (currentAmmo > 0)
        {
            GameObject magPrefab = Resources.Load<GameObject>(magName);
            ItemData magData = magPrefab?.GetComponent<ItemData>();
            int w = magData?.gridWidth ?? 1;
            int h = magData?.gridHeight ?? 2;
            inventory.AddItem(magName, w, h, currentAmmo);
            Debug.Log($"[Reload] 旧マガジン返却 ammo={currentAmmo}");
        }

        // 新マガジン装填
        int newAmmo = newMag.ammo;
        inventory.RemoveItemDirectly(newMag);
        currentAmmo = newAmmo;
        magazineLoaded = true;
        isReloading = false;
        Debug.Log($"[Reload] 完了 newAmmo={newAmmo}");
        hudManager?.UpdateAmmo(currentAmmo, currentWeapon.magazineSize);
    }
}