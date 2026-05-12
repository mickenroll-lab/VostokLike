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
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (resultManager == null)
            resultManager = FindObjectOfType<ResultManager>();
        if (spawnManager == null)
            spawnManager = FindObjectOfType<SpawnManager>();
        if (inventory == null)
            inventory = FindObjectOfType<Inventory>();

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Fキー押下");
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactRange))
            {
                Debug.Log("Raycastヒット: " + hit.collider.name + " tag: " + hit.collider.tag);

                if (hit.collider.CompareTag("ExitIn"))
                {
                    if (RaidManager.Instance == null) { Debug.LogError("RaidManager.Instance is null"); return; }
                    RaidManager.Instance.BeginRaid();
                    spawnManager.SpawnAtField();
                    resultManager.StartMission();
                }
                else if (hit.collider.CompareTag("ExitOut"))
                {
                    if (isExiting) return;
                    if (RaidManager.Instance == null) { Debug.LogError("RaidManager.Instance is null"); return; }
                    isExiting = true;
                    int totalValue = inventory.CalculateTotalValue();
                    resultManager.ShowResult(totalValue);
                    RaidManager.Instance.EndRaid(false);
                    isExiting = false;
                }
                else if (hit.collider.CompareTag("LootContainer"))
                {
                    Debug.Log("LootContainerインタラクト");
                    LootContainer lootContainer = hit.collider.GetComponent<LootContainer>();
                    if (lootContainer != null)
                        lootContainer.Interact();
                }
                else if (hit.collider.CompareTag("StorageContainer"))
                {
                    Debug.Log("StorageContainerインタラクト");
                    StorageContainer storageContainer = hit.collider.GetComponent<StorageContainer>();
                    if (storageContainer != null)
                        storageContainer.Interact();
                }
                else
                {
                    Debug.Log("一致しないタグ tag=[" + hit.collider.tag + "]");
                }
            }
        }
    }
}
