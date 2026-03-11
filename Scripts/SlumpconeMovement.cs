using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlumpconeMovement : MonoBehaviour
{
    public KeyCode moveKey = KeyCode.W; // Customize the key
    public float moveSpeed = 5.0f; // Customize the speed
    public float moveDistance = 1.0f; // Customize the distance

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
            Vector3 newPosition = transform.position + Vector3.up * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(initialPosition, newPosition) >= moveDistance)
            {
                isMoving = false;
                newPosition = initialPosition + Vector3.up * moveDistance;
            }

            transform.position = newPosition;
        }
    }
}
