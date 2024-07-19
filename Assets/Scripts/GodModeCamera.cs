using UnityEngine;

/// <summary>
/// GodModeCamera allows free-flying camera control.
/// </summary>
public class GodModeCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 10.0f;
    public float fastMovementSpeed = 50.0f;
    public float movementSmoothness = 0.1f;

    [Header("Rotation Settings")]
    public float lookSpeed = 2.0f;
    public float lookSmoothness = 0.1f;

    private Vector3 currentVelocity;
    private Vector3 targetVelocity;

    private Vector2 rotation = Vector2.zero;
    private Vector2 currentRotation;
    private Vector2 rotationSmoothVelocity;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovementInput();
        HandleMouseLook();
    }

    /// <summary>
    /// Handles input for camera movement.
    /// </summary>
    private void HandleMovementInput()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? fastMovementSpeed : movementSpeed;

        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

        // Vertical movement
        if (Input.GetKey(KeyCode.LeftControl))
        {
            direction.y = -1;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            direction.y = 1;
        }

        Vector3 targetMovement = (transform.forward * direction.z + transform.right * direction.x + transform.up * direction.y) * speed;

        targetVelocity = Vector3.Lerp(targetVelocity, targetMovement, movementSmoothness);
        transform.position += targetVelocity * Time.deltaTime;
    }

    /// <summary>
    /// Handles mouse look for camera rotation.
    /// </summary>
    private void HandleMouseLook()
    {
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -90, 90);

        currentRotation = Vector2.SmoothDamp(currentRotation, rotation, ref rotationSmoothVelocity, lookSmoothness);
        transform.eulerAngles = new Vector3(currentRotation.x, currentRotation.y, 0);
    }
}
