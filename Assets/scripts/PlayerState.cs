using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PlayerState : MonoBehaviour
{
    public float stamina = 100f;
    public float hunger = 100f;
    public float thirst = 100f;
    public float mental = 100f;

    public float staminaMax = 100f;
    public float hungerMax = 100f;
    public float thirstMax = 100f;
    public float mentalMax = 100f;

    public float hungerDecayRate = 0.1f;
    public float thirstDecayRate = 0.15f;
    public float staminaDecayRate = 5f;
    public float staminaRecoveryRate = 10f;

    public string currentItem = "";
    public int hp = 100;
    public int hpMax = 100;
    public GameObject deathPanel;
    public UnityEngine.UI.Image damageVignette;
    public TextMeshProUGUI mentalText;
    public Camera playerCamera;

    public bool isDead = false;
    public bool sleepBuffActive = false;
    private float hungerDamagePending = 0f;
    private float thirstDamagePending = 0f;
    private float mentalDamagePending = 0f;
    private float vignetteTimer = 0f;
    private const float vignetteDuration = 1f;
    private float defaultFOV = 60f;

    public void ResetState()
    {
        isDead = false;
        hp = hpMax;
        hunger = hungerMax;
        thirst = thirstMax;
        stamina = staminaMax;
        mental = mentalMax;
        hungerDamagePending = 0f;
        thirstDamagePending = 0f;
        mentalDamagePending = 0f;
        deathPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        MouseLook mouseLook = GetComponentInChildren<MouseLook>();
        if (mouseLook != null)
            mouseLook.ResetRotation();
    }

    public void ResumeAfterRaid()
    {
        isDead = false;
        hungerDamagePending = 0f;
        thirstDamagePending = 0f;
        mentalDamagePending = 0f;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        hp -= damage;
        mental = Mathf.Max(0f, mental - 5f);
        if (!SleepManager.IsSleeping)
            vignetteTimer = vignetteDuration;
        if (hp <= 0)
        {
            isDead = true;
            Die();
        }
    }

    void Start()
    {
        Debug.Log("deathPanel.activeSelf=" + deathPanel.activeSelf);
        PlayerState[] players = FindObjectsOfType<PlayerState>();
        if (players.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam != null) defaultFOV = cam.fieldOfView;

        if (damageVignette != null)
        {
            damageVignette.sprite = CreateVignetteSprite(256);
            damageVignette.color = new Color(1f, 0f, 0f, 0f);
        }
    }

    Sprite CreateVignetteSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(0.5f, 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 uv = new Vector2((float)x / size, (float)y / size);
                float dist = Vector2.Distance(uv, center) * 2f;
                dist = Mathf.Clamp01(dist);
                float alpha = dist * dist * dist;
                tex.SetPixel(x, y, new Color(1f, 0f, 0f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (SleepManager.IsSleeping) return;

        hunger -= hungerDecayRate * Time.deltaTime;
        thirst -= thirstDecayRate * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0, hungerMax);
        thirst = Mathf.Clamp(thirst, 0, thirstMax);

        // スタミナ増減
        bool hasMovement = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && hasMovement;
        if (isSprinting)
        {
            stamina -= staminaDecayRate * Time.deltaTime;
            thirst -= staminaDecayRate * 0.05f * Time.deltaTime;
        }
        else
            stamina += staminaRecoveryRate * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0f, staminaMax);

        // メンタル減少
        if (hunger < 40f) mental -= (1f / 20f) * Time.deltaTime;
        if (thirst < 40f) mental -= (1f / 20f) * Time.deltaTime;
        if (RaidManager.Instance != null && RaidManager.Instance.currentState == RaidManager.RaidState.InRaid)
        {
            float raidElapsed = (TimeManager.Instance != null)
                ? (TimeManager.Instance.currentTime - RaidManager.Instance.raidStartTime + 24f) % 24f
                : 0f;
            if (raidElapsed > 6f) mental -= (1f / 30f) * Time.deltaTime;
        }
        mental = Mathf.Clamp(mental, 0f, mentalMax);

        // メンタル0でダメージ
        if (mental <= 0f)
        {
            mentalDamagePending += Time.deltaTime;
            if (mentalDamagePending >= 1f)
            {
                mentalDamagePending -= 1f;
                TakeDamage(1);
            }
        }
        else
        {
            mentalDamagePending = 0f;
        }

        // FOV効果（mental < 20で狭める）
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam != null)
        {
            float targetFOV = mental < 20f ? defaultFOV - 20f : defaultFOV;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 2f);
        }

        // 飢餓・渇きダメージ
        bool bothEmpty = hunger <= 0 && thirst <= 0;
        if (hunger <= 0)
        {
            float rate = bothEmpty ? 1f : 0.5f;
            hungerDamagePending += rate * Time.deltaTime;
        }
        else hungerDamagePending = 0f;

        if (thirst <= 0)
        {
            float rate = bothEmpty ? 2f : 1f;
            thirstDamagePending += rate * Time.deltaTime;
        }
        else thirstDamagePending = 0f;

        if (hungerDamagePending >= 1f)
        {
            int dmg = (int)hungerDamagePending;
            hungerDamagePending -= dmg;
            TakeDamage(dmg);
        }
        if (thirstDamagePending >= 1f)
        {
            int dmg = (int)thirstDamagePending;
            thirstDamagePending -= dmg;
            TakeDamage(dmg);
        }

        // メンタルUI更新
        if (mentalText != null)
            mentalText.text = Mathf.CeilToInt(mental).ToString();

        UpdateVignette();
    }

    void UpdateVignette()
    {
        if (damageVignette == null) return;
        if (SleepManager.IsSleeping) return;
        if (vignetteTimer > 0f)
        {
            vignetteTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(vignetteTimer / vignetteDuration);
            damageVignette.color = new Color(1f, 0f, 0f, alpha);
        }
        else
        {
            damageVignette.color = new Color(1f, 0f, 0f, 0f);
        }
    }

    void Die()
    {
        vignetteTimer = 0f;
        if (damageVignette != null)
            damageVignette.color = new Color(1f, 0f, 0f, 0f);
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClearVignette()
    {
        vignetteTimer = 0f;
        if (damageVignette != null)
            damageVignette.color = new Color(1f, 0f, 0f, 0f);
    }

    public void Respawn()
    {
        RaidManager.Instance.EndRaid(true);
    }
}
