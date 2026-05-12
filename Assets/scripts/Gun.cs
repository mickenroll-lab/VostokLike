using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public Inventory inventory;
    public GameObject hitEffectPrefab;

    public ParticleSystem muzzleFlash;
    public ParticleSystem smokeEffect;
    public AudioClip gunShotSound;

    private WeaponData currentWeapon;
    private int currentAmmo;
    private bool isReloading = false;

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
        if (state.currentItem == "") return;

        if (inventory != null && inventory.inventoryPanel.activeSelf) return;
        if (state.deathPanel != null && state.deathPanel.activeSelf) return;

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
        if (muzzleFlash != null)
            muzzleFlash.Play();
        if (smokeEffect != null)
            smokeEffect.Play();
        if (audioSource != null && gunShotSound != null)
            audioSource.PlayOneShot(gunShotSound);

        if (currentWeapon == null) return;

        currentAmmo--;
        Debug.Log("残弾：" + currentAmmo);

        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
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