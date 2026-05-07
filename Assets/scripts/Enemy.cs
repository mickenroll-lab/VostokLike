using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp = 3;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log("残りHP：" + hp);

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}