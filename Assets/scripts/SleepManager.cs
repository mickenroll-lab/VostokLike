using UnityEngine;
using System.Collections;

public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance;
    public static bool IsSleeping = false;

    public GameObject blackPanel;
    public float sleepHours = 8f;

    private float lastSleepTime = -1f; // 負値=まだ一度も寝ていない

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

        // 前回睡眠から8時間経過チェック（24時跨ぎ考慮）
        if (lastSleepTime >= 0f && TimeManager.Instance != null)
        {
            float elapsed = (TimeManager.Instance.currentTime - lastSleepTime + 24f) % 24f;
            if (elapsed < sleepHours)
            {
                Debug.Log($"[SleepManager] 睡眠不可 前回から{elapsed:F1}時間しか経過していない（{sleepHours}時間必要）");
                return;
            }
        }

        StartCoroutine(SleepRoutine());
    }

    IEnumerator SleepRoutine()
    {
        IsSleeping = true;
        if (blackPanel != null) blackPanel.SetActive(true);

        Light directionalLight = TimeManager.Instance?.directionalLight;
        float originalIntensity = directionalLight != null ? directionalLight.intensity : 1f;
        if (directionalLight != null) directionalLight.intensity = 0f;

        TimeManager.Instance?.AdvanceTime(sleepHours);
        TimeManager.Instance?.ApplyLightAngle();

        PlayerState player = FindObjectOfType<PlayerState>();
        if (player != null)
        {
            // 累積防止：初回のみ最大値アップ
            if (!player.sleepBuffActive)
            {
                player.hpMax = Mathf.RoundToInt(player.hpMax * 1.2f);
                player.staminaMax *= 1.2f;
                player.sleepBuffActive = true;
            }

            // HUN・THIを30消費（回復ではなくコスト）
            player.hunger = Mathf.Max(0f, player.hunger - 30f);
            player.thirst = Mathf.Max(0f, player.thirst - 30f);
            player.mental = Mathf.Min(player.mental + 50f, player.mentalMax);
            player.ClearVignette();
        }

        lastSleepTime = TimeManager.Instance?.currentTime ?? 0f;

        yield return new WaitForSecondsRealtime(1f);

        if (directionalLight != null) directionalLight.intensity = originalIntensity;
        if (blackPanel != null) blackPanel.SetActive(false);
        IsSleeping = false;
    }
}
