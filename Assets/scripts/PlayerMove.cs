using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 3f;
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

        if (IsGrounded())
        {
            if (yVelocity < 0)
                yVelocity = -2f;

            bool canJump = playerState == null || playerState.stamina > 0;
            if (Input.GetKeyDown(KeyCode.Space) && canJump)
            {
                yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (playerState != null)
                    playerState.stamina = Mathf.Max(0f, playerState.stamina - 10f);
            }
        }

        yVelocity += gravity * Time.deltaTime;
        Vector3 velocity = new Vector3(0, yVelocity, 0);
        controller.Move((move * currentSpeed + velocity) * Time.deltaTime);

        float speedValue = Mathf.Abs(x) + Mathf.Abs(z);
        animator.SetFloat("Speed", speedValue);
    }

    bool IsGrounded()
    {
        if (controller.isGrounded) return true;
        // controller.isGrounded は底面がわずかに浮くと false になるためRaycastで補完
        float checkLength = controller.height / 2f + 0.3f;
        return Physics.Raycast(transform.position, Vector3.down, checkLength, ~LayerMask.GetMask("Player"));
    }
}
