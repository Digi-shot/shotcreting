using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRightScript : MonoBehaviour
{
    public KeyCode moveKey = KeyCode.D; // Customize the key
    public float moveSpeed = 5.0f; // Customize the speed
    public float moveDistance = 1.0f; // Customize the distance
    public KeyCode stopKey = KeyCode.A; // Key to stop the movement
    private bool isMoving = false;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(moveKey))
        {
            isMoving = true;
        }

        if (isMoving)
        {
            Vector3 newPosition = transform.position + Vector3.right * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(initialPosition, newPosition) >= moveDistance)
            {
                isMoving = false;
                newPosition = initialPosition + Vector3.right * moveDistance;
            }

            transform.position = newPosition;
        }

        // Check for the stopKey to stop the movement
        if (Input.GetKeyDown(stopKey))
        {
            isMoving = false;
        }

        if (!isMoving)
        {
            // Your code for stopping, if needed
            // For example, you can add code here to perform any additional actions when movement stops.
        }
    }
}
