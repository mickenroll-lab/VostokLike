using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 1.5f;
    public CharacterController controller;
    public float gravity = -9.8f;
    public float yVelocity;
    public Animator animator;

    private PlayerState playerState;

    void Awake()
    {
        playerState = GetComponent<PlayerState>();
    }

    void Update()
    {
        if (SleepManager.IsSleeping) return;

        Inventory inventory = GetComponent<Inventory>();
        if (inventory != null && inventory.inventoryPanel.activeSelf)
            return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        bool canSprint = playerState == null || playerState.stamina > 0;
        float currentSpeed = (Input.GetKey(KeyCode.LeftShift) && canSprint) ? sprintSpeed : speed;
        if (playerState != null && playerState.mental < 40f) currentSpeed *= 0.8f;

        if (controller.isGrounded)
        {
            if (yVelocity < 0)
                yVelocity = -2f;

            if (Input.GetKeyDown(KeyCode.Space))
                yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        yVelocity += gravity * Time.deltaTime;
        Vector3 velocity = new Vector3(0, yVelocity, 0);
        controller.Move((move * currentSpeed + velocity) * Time.deltaTime);

        float speedValue = Mathf.Abs(x) + Mathf.Abs(z);
        animator.SetFloat("Speed", speedValue);
    }
}
