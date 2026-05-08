using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public int enemyCount = 5;

    void Start()
    {
        SpawnEnemies();
    }

    public void ResetEnemies()
    {
        Debug.Log("ResetEnemies実行");
        StartCoroutine(ResetEnemiesCoroutine());
    }

    System.Collections.IEnumerator ResetEnemiesCoroutine()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
            Destroy(enemy);

        Debug.Log("敵削除完了");
        yield return new WaitForSeconds(1f);

        SpawnEnemies();
        Debug.Log("敵再スポーン完了");
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}