using UnityEngine;

public class TamperMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;          // Horizontal movement speed
    public float verticalSpeed = 2.0f;      // Vertical movement speed
    public float maxRadius = 0.05f;         // Maximum allowed radius in the XZ plane (meters)

    private Vector3 initialPosition;        // Initial position used as the center of the allowed XZ movement area

    void Start()
    {
        // Record the initial position as the center of the circular XZ boundary
        initialPosition = transform.position;
    }

    void Update()
    {
        // Get horizontal and vertical input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Create movement direction in local space
        Vector3 moveDirection = new Vector3(horizontalInput, 0.0f, verticalInput);

        // Convert movement direction to world space
        moveDirection = transform.TransformDirection(moveDirection);

        // Calculate vertical movement input
        float verticalMovement = 0.0f;
        if (Input.GetKey(KeyCode.R))
        {
            verticalMovement = 1.0f;
        }
        else if (Input.GetKey(KeyCode.F))
        {
            verticalMovement = -1.0f;
        }

        // Calculate frame-based movement
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
        Vector3 verticalMovementVector = Vector3.up * verticalMovement * verticalSpeed * Time.deltaTime;

        // Apply movement
        transform.Translate(movement, Space.World);
        transform.Translate(verticalMovementVector, Space.World);

        // Limit XZ position within a circle centered at the initial position
        Vector3 currentPosition = transform.position;
        Vector2 offsetXZ = new Vector2(
            currentPosition.x - initialPosition.x,
            currentPosition.z - initialPosition.z
        );

        if (offsetXZ.magnitude > maxRadius)
        {
            offsetXZ = offsetXZ.normalized * maxRadius;
            transform.position = new Vector3(
                initialPosition.x + offsetXZ.x,
                currentPosition.y,
                initialPosition.z + offsetXZ.y
            );
        }
    }
}
