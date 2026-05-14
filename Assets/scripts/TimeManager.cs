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

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        currentTime += Time.deltaTime * gameTimeScale / 3600f;
        if (currentTime >= 24f) currentTime -= 24f;
        UpdateLight();
        UpdateUI();
    }

    void UpdateLight()
    {
        if (directionalLight == null) return;
        // 6:00=0deg, 12:00=90deg, 18:00=180deg, 0:00=270deg
        float angle = (currentTime - 6f) * 15f;
        Vector3 euler = directionalLight.transform.eulerAngles;
        directionalLight.transform.rotation = Quaternion.Euler(angle, euler.y, euler.z);
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
        if (currentTime >= 24f) currentTime -= 24f;
    }
}
