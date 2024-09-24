using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed at which the camera moves
    private Vector3 startingPosition; // To store the initial position of the camera

    void Start()
    {
        // Store the starting position of the camera when the scene starts
        startingPosition = transform.position;
    }

    void Update()
    {
        // Move the camera up with "W"
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        }

        // Move the camera down with "S"
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        }

        // Move the camera left with "A"
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }

        // Move the camera right with "D"
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

        // Reset the camera position to the starting position with the spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = startingPosition;
        }
    }
}
