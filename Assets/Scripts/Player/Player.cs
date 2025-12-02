using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float gravity = -15f;
    public float jumpHeight = 2f;

    [Header("Camera")]
    public Camera mainCamera;
    public float mouseSensitivity = 0.3f;
    public Transform cameraTransform;
    private float pitch = 0f;

    [Header("Camera zoom effects")]
    public float normalFOV = 90f;
    public float defaultZoomedFOV = 30f; 
    public float minZoomFOV = 10f;  
    public float maxZoomFOV = 60f; 
    public float zoomStep = 5f;    
    public float lerpSpeed = 10f;

    private float currentAdjustedZoomFOV;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        LockCursor();
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = normalFOV;
            currentAdjustedZoomFOV = defaultZoomedFOV; 
        }
    }

    void Update()
    {
        HandleCursorLockToggle();

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleMovementAndGravity();
            HandleMouseLook();
            CameraZoom();
        }
    }

    private void HandleMovementAndGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (transform.forward * v + transform.right * h).normalized;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up, mouseX, Space.World);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void HandleCursorLockToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    private void CameraZoom()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                currentAdjustedZoomFOV -= scroll * zoomStep;
                currentAdjustedZoomFOV = Mathf.Clamp(currentAdjustedZoomFOV, minZoomFOV, maxZoomFOV);
            }
        }

        float targetFOV = Input.GetKey(KeyCode.Z) ? currentAdjustedZoomFOV : normalFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, lerpSpeed * Time.deltaTime);
    }
}
