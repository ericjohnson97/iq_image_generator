using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public List<Camera> cameras = new List<Camera>();
    private int currentCameraIndex;

    void Start()
    {
        currentCameraIndex = 0;

        // Initially, disable all cameras except the first one
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] != null)
            {
                cameras[i].enabled = (i == currentCameraIndex);
            }
        }
        
        if (cameras.Count > 0 && cameras[0] != null)
        {
            Debug.Log("Camera with name: " + cameras[0].name + ", is now rendering.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Disable the currently enabled camera
            if (currentCameraIndex < cameras.Count && cameras[currentCameraIndex] != null)
            {
                cameras[currentCameraIndex].enabled = false;
            }

            // Move to the next camera
            currentCameraIndex++;
            if (currentCameraIndex >= cameras.Count)
            {
                currentCameraIndex = 0;
            }

            // Enable the new current camera
            if (cameras[currentCameraIndex] != null)
            {
                cameras[currentCameraIndex].enabled = true;
                Debug.Log("Camera with name: " + cameras[currentCameraIndex].name + ", is now rendering.");
            }
        }
    }

    public void AddCamera(Camera newCamera)
    {
        if (newCamera != null)
        {
            cameras.Add(newCamera);
            // By default, new cameras are added as disabled
            // newCamera.enabled = false;
        }
        else
        {
            Debug.LogError("Attempted to add a null camera to the CameraController.");
        }
    }
}
