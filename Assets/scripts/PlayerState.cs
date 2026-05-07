using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PlayerState : MonoBehaviour
{
    public string currentItem = "";
    public int hp = 100;
    public GameObject deathPanel;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log("プレイヤーHP：" + hp);

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Respawn()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}