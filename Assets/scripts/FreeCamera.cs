using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float mouseSensitivity = 10f; // ←ここ上げた
    public float boostMultiplier = 3f;

    float rotationX = 0f;
    float rotationY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ===== マウス視点 =====
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);

        // ===== 移動速度 =====
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= boostMultiplier;
        }

        // ===== 水平移動（ここが重要） =====
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 前方向（Y無視）
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        // 右方向（Y無視）
        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        Vector3 move = forward * v + right * h;

        transform.position += move * speed * Time.deltaTime;

        // ===== 高さ変更だけ別操作 =====
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }
    }
}