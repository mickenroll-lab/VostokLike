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

    public float hungerDecayRate = 0.1f;
    public float thirstDecayRate = 0.15f;
    public float staminaDecayRate = 5f;
    public float staminaRecoveryRate = 10f;

    public string currentItem = "";
    public int hp = 100;
    public GameObject deathPanel;

    private bool isDead = false;
    private float hungerDamagePending = 0f;
    private float thirstDamagePending = 0f;

    public void ResetState()
    {
        isDead = false;
        hp = 100;
        hunger = hungerMax;
        thirst = thirstMax;
        stamina = staminaMax;
        hungerDamagePending = 0f;
        thirstDamagePending = 0f;
        deathPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        MouseLook mouseLook = GetComponentInChildren<MouseLook>();
        if (mouseLook != null)
            mouseLook.ResetRotation();
    }

    // 帰還時：ステータスを引き継いでカーソルとタイムスケールだけ戻す
    public void ResumeAfterRaid()
    {
        isDead = false;
        hungerDamagePending = 0f;
        thirstDamagePending = 0f;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        hp -= damage;
        Debug.Log("プレイヤーHP：" + hp);
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
    }

    void Update()
    {
        hunger -= hungerDecayRate * Time.deltaTime;
        thirst -= thirstDecayRate * Time.deltaTime;

        hunger = Mathf.Clamp(hunger, 0, hungerMax);
        thirst = Mathf.Clamp(thirst, 0, thirstMax);

        // スタミナ増減（LeftShift + 移動入力があるときのみ減少）
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

        // 飢餓・渇きダメージ（フレームレート非依存）
        bool bothEmpty = hunger <= 0 && thirst <= 0;

        if (hunger <= 0)
        {
            float rate = bothEmpty ? 1f : 0.5f;
            hungerDamagePending += rate * Time.deltaTime;
        }
        else
        {
            hungerDamagePending = 0f;
        }

        if (thirst <= 0)
        {
            float rate = bothEmpty ? 2f : 1f;
            thirstDamagePending += rate * Time.deltaTime;
        }
        else
        {
            thirstDamagePending = 0f;
        }

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
    }

    void Die()
    {
        Debug.Log("Die呼ばれた deathPanel=" + deathPanel);
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Respawn()
    {
        RaidManager.Instance.EndRaid(true);
    }

}
