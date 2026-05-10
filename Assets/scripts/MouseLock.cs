using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public Inventory inventory;

    public float mouseSensitivity = 300f;
    public Transform playerBody;
    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ResetRotation()
    {
        xRotation = 0f;
        transform.localRotation = Quaternion.identity;
    }

    void Update()
    {
        PlayerState playerState = playerBody.GetComponent<PlayerState>();
        // à»â∫ä˘ë∂èàóù
        ResultManager resultManager = FindObjectOfType<ResultManager>();

        if ((inventory != null && inventory.inventoryPanel.activeSelf) ||
            (playerState != null && playerState.deathPanel.activeSelf) ||
            (resultManager != null && resultManager.resultPanel.activeSelf))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}