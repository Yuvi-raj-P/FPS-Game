using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float speed = 5f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;
    bool sprinting = false;
    private bool isGrounded;
    public float walkSpeed = 8f;
    public float sprintSpeed = 12f;
    public float scopeSpeed = 3f;
    private bool isScoped = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        speed = walkSpeed;

    }

    // Update is called once per frame
    void Update()
    {

        isGrounded = controller.isGrounded;
    }

    public void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
       
    }
    public void Jump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }
    public void Sprint(bool isSprinting)
    {
        sprinting = isSprinting;
        if (!isScoped)
        {
            speed = isSprinting ? sprintSpeed : walkSpeed;
        }
    }
    public void SetScoped(bool scoped)
    {
        isScoped = scoped;
        if (scoped)
        {
            speed = scopeSpeed;
        }
        else
        {
            speed = sprinting ? sprintSpeed : walkSpeed;
        }
        isScoped = scoped;
        
    }
}
