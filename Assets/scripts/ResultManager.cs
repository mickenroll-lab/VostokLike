using UnityEngine;
using TMPro;

public class ResultManager : MonoBehaviour
{
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public EnemySpawnManager enemySpawnManager;
    public SpawnManager spawnManager;

    private int enemiesKilled = 0;
    private float survivalTime = 0f;
    private bool isMission = false;

    void Update()
    {
        if (isMission)
            survivalTime += Time.deltaTime;
    }

    public void StartMission()
    {
        enemiesKilled = 0;
        survivalTime = 0f;
        isMission = true;
    }

    public void AddKill()
    {
        enemiesKilled++;
    }

    public void ShowResult(int totalValue)
    {
        isMission = false;

        int minutes = (int)survivalTime / 60;
        int seconds = (int)survivalTime % 60;

        resultText.text =
            "--- RESULT ---\n\n" +
            "Value: " + totalValue + "\n" +
            "Kills: " + enemiesKilled + "\n" +
            "Time: " + minutes + ":" + seconds.ToString("00");

        resultPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnOKButton()
    {
        resultPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        enemySpawnManager.ResetEnemies();
        spawnManager.SpawnAtSafe();
    }
}