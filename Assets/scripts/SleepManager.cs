using UnityEngine;

public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance;

    public float fadeDuration = 1f;
    public float sleepHours = 8f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (ScreenFade.Instance == null)
            new GameObject("ScreenFade").AddComponent<ScreenFade>();
    }

    public void Sleep()
    {
        Debug.Log($"[SleepManager] Sleep呼ばれた回数確認 isSleeping={ScreenFade.State == ScreenState.Sleeping} Time={Time.time:F2} State={ScreenFade.State}");
        if (ScreenFade.State != ScreenState.Normal)
        {
            Debug.LogWarning($"[SleepManager] Stateが{ScreenFade.State}のためスキップ");
            return;
        }
        if (ScreenFade.Instance == null)
        {
            Debug.LogError("[SleepManager] ScreenFade.Instance が null");
            return;
        }
        ScreenFade.State = ScreenState.Sleeping;

        PlayerState player = FindObjectOfType<PlayerState>();

        // 問題A：フェード開始前に赤ビネットを即消去
        player?.ClearVignette();

        Debug.Log("[SleepManager] FadeOutIn呼び出し");
        ScreenFade.Instance.FadeOutIn(fadeDuration,
            onBlack: () =>
            {
                Debug.Log("[SleepManager] onBlack: 時刻スキップ実行");
                TimeManager.Instance?.AdvanceTime(sleepHours);
            },
            onComplete: () =>
            {
                Debug.Log("[SleepManager] onComplete: 睡眠ボーナス適用");
                if (player != null)
                {
                    player.hpMax = Mathf.RoundToInt(player.hpMax * 1.2f);
                    player.hp = Mathf.Min(player.hp, player.hpMax);
                    player.staminaMax *= 1.2f;
                    player.stamina = player.staminaMax;
                    // 問題B：睡眠後の即時hunger/thirstダメージを防ぐため回復
                    player.hunger = player.hungerMax;
                    player.thirst = player.thirstMax;
                    player.ClearVignette();
                }
                ScreenFade.State = ScreenState.Normal;
                Debug.Log("[SleepManager] 睡眠完了 State=Normal");
            }
        );
    }
}
