using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private InputSystem_Actions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private bool isSprinting;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Mouse Look & Camera")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;
    
    // Camera toggle variables
    public bool isFirstPerson = true;
    public Vector3 firstPersonLocalPos = new Vector3(0f, 0.6f, 0f); // Adjust to eye level
    public Vector3 thirdPersonLocalPos = new Vector3(0f, 1.5f, -3f); // Adjust for over-the-shoulder

    [Header("Animations")]
    private Animator animator;

    private float xRotation = 0f;
    private Vector3 velocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();
        animator = GetComponent<Animator>(); 
    }

    private void OnEnable()
    {
        inputActions.Enable();

        // Movement
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Look
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // Jump
        inputActions.Player.Jump.performed += ctx => Jump();

        // Sprint (Assuming you add a "Sprint" action to your Input Map)
        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

        // Switch Camera (Assuming you add a "SwitchCamera" action to your Input Map)
        inputActions.Player.SwitchCamera.performed += ctx => ToggleCamera();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        // Initialize camera to the correct position on start
        UpdateCameraPosition();
    }

    private void Update()
    {
        Move();
        Look();
        ApplyGravity();
        UpdateAnimation(); 
    }

    private void Move()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Determine current speed based on sprint state
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void ApplyGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ToggleCamera()
    {
        isFirstPerson = !isFirstPerson;
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        // Smoothly interpolating this later might look better, but snapping is fine for now
        cameraTransform.localPosition = isFirstPerson ? firstPersonLocalPos : thirdPersonLocalPos;
    }

    private void UpdateAnimation()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        // Break running into walk and sprint for the Animator
        animator.SetBool("isWalking", isMoving && !isSprinting);
        animator.SetBool("isRunning", isMoving && isSprinting);

        // Sideways movement logic 
        bool turningLeft = moveInput.x < -0.1f;
        bool turningRight = moveInput.x > 0.1f;

        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("TurnLeft", turningLeft);
        animator.SetBool("TurnRight", turningRight);
    }
}