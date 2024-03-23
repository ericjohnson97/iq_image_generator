using System;
using UnityEngine;

/// <summary>
/// increments view through cameras
/// </summary>


public class CameraController : MonoBehaviour
{
    public Camera[] cameras;
    private int currentCameraIndex;

    // Start is called before the first frame update
    void Start()
    {
        currentCameraIndex = 0;

        // Assume each camera is set up to render into a RenderTexture.
        // Your streaming logic would then capture frames from these RenderTextures,
        // rather than directly from the camera's viewport.
        foreach (var cam in cameras)
        {
            var renderTexture = new RenderTexture(1920, 1080, 24);
            cam.targetTexture = renderTexture;
            // Set up streaming to capture from `cam.targetTexture`
        }


        // If any cameras were added to the controller, enable the first one
        if (cameras.Length > 0)
        {
            cameras[0].gameObject.SetActive(true);
            Debug.Log("Camera with name: " + cameras[0].name + ", is now enabled");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Press the 'C' key to cycle through cameras in the array
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Cycle to the next camera
            currentCameraIndex++;

            // If cameraIndex is in bounds, set this camera active and last one inactive
            if (currentCameraIndex < cameras.Length)
            {
                // cameras[currentCameraIndex - 1].gameObject.SetActive(false);
                cameras[currentCameraIndex].gameObject.SetActive(true);
                Debug.Log("Camera with name: " + cameras[currentCameraIndex].name + ", is now enabled");
            }
            // If last camera, cycle back to first camera
            else
            {
                // cameras[currentCameraIndex - 1].gameObject.SetActive(false);
                currentCameraIndex = 0;
                cameras[currentCameraIndex].gameObject.SetActive(true);
                Debug.Log("Camera with name: " + cameras[currentCameraIndex].name + ", is now enabled");
            }
        }
    }
}