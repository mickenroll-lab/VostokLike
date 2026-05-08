using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp = 110;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log("残りHP：" + hp);
        if (hp <= 0)
        {
            ResultManager resultManager = FindObjectOfType<ResultManager>();
            if (resultManager != null)
                resultManager.AddKill();

            Destroy(gameObject);
        }
    }
}