using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public bool shouldRotate = true;

    // The target we are following
    public Transform target;

    private float xRotationOffset = 0;
    private float yRotationOffset = 0;

    private Quaternion currentRotation;
    private Vector3 offsetPosition, finalPosition;

    // Rotation and zoom speed
    public float rotationSpeed = 100.0f; // Adjust rotation speed as needed
    public float zoomSpeed = 10.0f; // Adjust zoom speed as needed

    private void Start()
    {
        offsetPosition = new Vector3(0, 5, -10);
    }

    private void LateUpdate()
    {
        if (!target)
            return;

        float ScrollWheelChange = Input.GetAxis("Mouse ScrollWheel");
        if (ScrollWheelChange != 0)
        {
            // Make zoom speed frame rate independent
            offsetPosition = offsetPosition + new Vector3(0, 0, ScrollWheelChange * zoomSpeed * Time.deltaTime);
        }
        if (Input.GetKey("w"))
        {
            // Adjust rotation with respect to Time.deltaTime
            xRotationOffset += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s"))
        {
            xRotationOffset -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey("a"))
        {
            yRotationOffset += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey("d"))
        {
            yRotationOffset -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey("r"))
        {
            yRotationOffset = 0;
            xRotationOffset = 0;
            offsetPosition = new Vector3(0, 5, -10);
        }

        // Calculate the current rotation angles
        float wantedRotationAngle = target.eulerAngles.y + yRotationOffset;
        float currentRotationAngle = transform.eulerAngles.y;

        // Damp the rotation around the y-axis
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, Time.deltaTime * 3f); // Using Time.deltaTime to smooth the rotation over time

        // Convert the angle into a rotation
        currentRotation = Quaternion.Euler(xRotationOffset, currentRotationAngle, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        transform.position = target.position + (currentRotation * offsetPosition);

        // Always look at the target
        transform.LookAt(target);
    }
}
