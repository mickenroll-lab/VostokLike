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
        // �ȉ���������

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F�L�[������");
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactRange))
            {
                Debug.Log("Raycast�q�b�g: " + hit.collider.name + " tag: " + hit.collider.tag);
                if (hit.collider.CompareTag("ExitIn"))
                {
                    RaidManager.Instance.BeginRaid();
                    spawnManager.SpawnAtField();
                    resultManager.StartMission();
                }
                else if (hit.collider.CompareTag("ExitOut"))
                {
                    if (isExiting) return;
                    isExiting = true;
                    int totalValue = inventory.CalculateTotalValue();
                    resultManager.ShowResult(totalValue);
                    RaidManager.Instance.EndRaid(false);
                    isExiting = false;
                }
                if (hit.collider.tag == "LootContainer")
                {
                    Debug.Log("LootContainer������");
                    LootContainer lootContainer = hit.collider.GetComponent<LootContainer>();
                    Debug.Log("LootContainer�擾: " + lootContainer);
                    if (lootContainer != null)
                        lootContainer.Interact();
                }
                else
                {
                    Debug.Log("��v���Ȃ����� tag=[" + hit.collider.tag + "]");
                }
            }
        }

    }
} 