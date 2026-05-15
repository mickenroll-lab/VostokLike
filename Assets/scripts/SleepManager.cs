using UnityEngine;
using System.Collections;

public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance;
    public static bool IsSleeping = false;

    public GameObject blackPanel;
    public float sleepHours = 8f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        IsSleeping = false;
        if (blackPanel != null) blackPanel.SetActive(false);
    }

    public void Sleep()
    {
        if (IsSleeping) return;
        PlayerState player = FindObjectOfType<PlayerState>();
        if (player != null && player.isDead) return;
        StartCoroutine(SleepRoutine());
    }

    IEnumerator SleepRoutine()
    {
        IsSleeping = true;
        if (blackPanel != null) blackPanel.SetActive(true);

        // Lightを消してから時刻変更・角度適用
        Light directionalLight = TimeManager.Instance?.directionalLight;
        float originalIntensity = directionalLight != null ? directionalLight.intensity : 1f;
        if (directionalLight != null) directionalLight.intensity = 0f;

        TimeManager.Instance?.AdvanceTime(sleepHours);
        TimeManager.Instance?.ApplyLightAngle();

        PlayerState player = FindObjectOfType<PlayerState>();
        if (player != null)
        {
            player.hpMax = Mathf.RoundToInt(player.hpMax * 1.2f);
            player.hp = Mathf.Min(player.hp, player.hpMax);
            player.staminaMax *= 1.2f;
            player.stamina = player.staminaMax;
            player.hunger = player.hungerMax;
            player.thirst = player.thirstMax;
            player.ClearVignette();
        }

        yield return new WaitForSecondsRealtime(1f);

        // Lightを元の強度に戻してからblackPanel非表示
        if (directionalLight != null) directionalLight.intensity = originalIntensity;
        if (blackPanel != null) blackPanel.SetActive(false);
        IsSleeping = false;
    }
}
