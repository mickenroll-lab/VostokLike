using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private Color[] originalColors;

    public int hp = 110;

    // フィールドに追加
    private Renderer[] renderers;
    private Vector3 knockbackVelocity;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;
    }

    void Update()
    {
        // ノックバックの減衰
        if (knockbackVelocity.magnitude > 0.01f)
        {
            transform.position += knockbackVelocity * Time.deltaTime;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 10f);
        }
    }



    IEnumerator HitFlash()
    {
        if (renderers == null || renderers.Length == 0) yield break;

        // 赤くする
        foreach (var r in renderers)
            // 以下そのまま
            foreach (var mat in r.materials)
                mat.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        // 元の色に戻す
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = originalColors[i];
    }


    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log("残りHP：" + hp);

        // ヒットリアクション
        StartCoroutine(HitFlash());
        knockbackVelocity = -transform.forward * 2f;

        
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
            ai.OnDamaged();

        if (hp <= 0)
        {
            ResultManager resultManager = FindObjectOfType<ResultManager>();
            if (resultManager != null)
                resultManager.AddKill();
            OnDeath();
        }
    }

    void OnDeath()
    {
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.enabled = false;

        gameObject.tag = "Corpse";

        enabled = false;
    }
}