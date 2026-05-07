using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitZone : MonoBehaviour
{
    public float interactRange = 3f;
    public Camera playerCamera;
    public SpawnManager spawnManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange))
            {
                Debug.Log("当たった：" + hit.collider.tag);
                if (hit.collider.CompareTag("ExitIn"))
                {
                    Debug.Log("出撃！");
                    spawnManager.SpawnAtField();
                }
                else if (hit.collider.CompareTag("ExitOut"))
                {
                    // 外から中へ（帰還）
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }
}