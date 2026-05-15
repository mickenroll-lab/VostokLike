using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Time Settings")]
    public float gameTimeScale = 60f;
    public float currentTime = 8f;

    [Header("References")]
    public Light directionalLight;
    public TextMeshProUGUI timeText;

    private float lightYAngle = -30f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (directionalLight != null)
            lightYAngle = directionalLight.transform.eulerAngles.y;
        UpdateLight();
    }

    void Update()
    {
        if (SleepManager.IsSleeping) return;

        currentTime += Time.deltaTime * gameTimeScale / 3600f;
        if (currentTime >= 24f) currentTime -= 24f;
        UpdateLight();
        UpdateUI();
    }

    void UpdateLight()
    {
        if (directionalLight == null) return;
        float angle = GetSunAngle();
        // ジンバルロック回避：Y軸固定 + X軸AngleAxisで仰角制御
        directionalLight.transform.rotation =
            Quaternion.AngleAxis(lightYAngle, Vector3.up) *
            Quaternion.AngleAxis(angle, Vector3.right);
    }

    float GetSunAngle()
    {
        // 0:00=270deg, 6:00=0deg, 12:00=90deg, 18:00=180deg, 24:00=270deg
        // currentTime/24 で0〜1に正規化し360度循環させることで24時跨ぎの急ジャンプを防ぐ
        return (currentTime / 24f * 360f + 270f) % 360f;
    }

    void UpdateUI()
    {
        if (timeText == null) return;
        int h = (int)currentTime;
        int m = (int)((currentTime - h) * 60f);
        timeText.text = $"{h:00}:{m:00}";
    }

    public void AdvanceTime(float hours)
    {
        currentTime += hours;
        while (currentTime >= 24f) currentTime -= 24f;
        while (currentTime < 0f) currentTime += 24f;
    }

    public void ApplyLightAngle()
    {
        UpdateLight();
    }
}
