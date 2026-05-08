using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitZone : MonoBehaviour
{
    public float interactRange = 3f;
    public Camera playerCamera;
    public SpawnManager spawnManager;
    public Inventory inventory;
    public EnemySpawnManager enemySpawnManager;
    public ResultManager resultManager;

    private bool isExiting = false;

    void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange))
            {
                if (hit.collider.CompareTag("ExitIn"))
                {
                    spawnManager.SpawnAtField();
                    resultManager.StartMission();
                }
                else if (hit.collider.CompareTag("ExitOut"))
                {
                    if (isExiting) return;
                    isExiting = true;
                    int totalValue = inventory.CalculateTotalValue();
                    spawnManager.SpawnAtSafe();
                    resultManager.ShowResult(totalValue);
                    isExiting = false;
                }
            }
        }
    }
}