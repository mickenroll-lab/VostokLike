using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;


public class PlayerState : MonoBehaviour
{
    public float stamina = 100f;
    public float hunger = 100f;
    public float thirst = 100f;

    public float staminaMax = 100f;
    public float hungerMax = 100f;
    public float thirstMax = 100f;

    public float hungerDecayRate = 0.1f;  // 1秒あたりの減少量
    public float thirstDecayRate = 0.15f; 
    
    public string currentItem = "";
    public int hp = 100;
    public GameObject deathPanel;

    // フィールドに追加
    private bool isDead = false;

    public void ResetState()
    {
        isDead = false;
        hp = 100;
        hunger = hungerMax;
        thirst = thirstMax;
        deathPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        MouseLook mouseLook = GetComponentInChildren<MouseLook>();
        if (mouseLook != null)
            mouseLook.ResetRotation();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // ← 追加
        hp -= damage;
        Debug.Log("プレイヤーHP：" + hp);
        if (hp <= 0)
        {
            isDead = true; // ← 追加
            Die();
        }
    }

    void Start()
    {
        Debug.Log("deathPanel.activeSelf=" + deathPanel.activeSelf);
        // 既に存在する場合は自分を削除
        PlayerState[] players = FindObjectsOfType<PlayerState>();
        if (players.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        // 飢えと渇きは時間で減少
        hunger -= hungerDecayRate * Time.deltaTime;
        thirst -= thirstDecayRate * Time.deltaTime;

        hunger = Mathf.Clamp(hunger, 0, hungerMax);
        thirst = Mathf.Clamp(thirst, 0, thirstMax);

        // 0になったらダメージ
        if (hunger <= 0)
            TakeDamage(1);
        if (thirst <= 0)
            TakeDamage(2);
    }
    void Die()
    {
        Debug.Log("Die呼ばれた deathPanel=" + deathPanel);
        deathPanel.SetActive(true); // ← 1つだけ残す
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Respawn()
    {
        RaidManager.Instance.EndRaid(true);
    }
    
}