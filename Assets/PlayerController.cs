using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float defaultMoveSpeed = 3f; // Default movement speed
    [SerializeField] private float sprintMoveSpeed = 8f; // Sprinting movement speed
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float jumpForce = 5f;

    private CharacterController characterController;
    private Camera playerCamera;
    private Vector3 moveDirection;
    private float verticalRotation = 0f;
    private float verticalVelocity = 0f;
    private float horizontalRotation = 0f;
    private float defaultFOV;
    private bool isSprinting = false;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        defaultFOV = playerCamera.fieldOfView; // Store the default FOV
    }

    private void Update()
    {
        // Player movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Check if sprinting
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.W))
        {
            moveDirection = transform.TransformDirection(new Vector3(moveHorizontal, 0, moveVertical)) * sprintMoveSpeed;
            isSprinting = true;
        }
        else
        {
            moveDirection = transform.TransformDirection(new Vector3(moveHorizontal, 0, moveVertical)) * defaultMoveSpeed;
            isSprinting = false;
        }

        // Jumping
        if (characterController.isGrounded)
        {
            verticalVelocity = -1f; // Reset vertical velocity when grounded

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        moveDirection.y = verticalVelocity;

        characterController.Move(moveDirection * Time.deltaTime);

        // Check if below teleportation threshold
        if (transform.position.y < -64f)
        {
            TeleportToPosition(new Vector3(33f, 25f, 33f));
            verticalVelocity = 0f; // Reset vertical velocity after teleporting
        }

        // Player rotation
        float lookHorizontal = Input.GetAxis("Mouse X");
        float lookVertical = Input.GetAxis("Mouse Y");

        horizontalRotation += lookHorizontal * mouseSensitivity;
        verticalRotation -= lookVertical * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);

        // Adjust FOV for sprinting
        if (isSprinting)
        {
            playerCamera.fieldOfView = defaultFOV + 3f; // Increase FOV by 3
        }
        else
        {
            playerCamera.fieldOfView = defaultFOV; // Reset FOV to default
        }

        // Teleportation
        if (Input.GetKeyDown(KeyCode.R))
        {
            TeleportToPosition(new Vector3(33f, 25f, 33f));
            verticalVelocity = 0f; // Reset vertical velocity after teleporting
        }
        // Esc to Quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void TeleportToPosition(Vector3 position)
    {
        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;
    }
}
