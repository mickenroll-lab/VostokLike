using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public int enemyCount = 5;
    public Transform enemiesRoot;

    void Start()
    {
        SpawnEnemies();
    }

    public void ResetEnemies()
    {
        Debug.Log("ResetEnemies���s");
        StartCoroutine(ResetEnemiesCoroutine());
    }

    System.Collections.IEnumerator ResetEnemiesCoroutine()
    {
        if (enemiesRoot != null)
        {
            foreach (Transform child in enemiesRoot)
                Destroy(child.gameObject);
        }
        else
        {
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                Destroy(enemy);
        }

        yield return new WaitForSeconds(1f);

        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation, enemiesRoot);
        }
    }
}