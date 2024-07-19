using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public List<Camera> cameras = new List<Camera>();
    public GameObject godModeObject;
    private int currentCameraIndex;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;

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
            godModeObject.SetActive(false);
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

        if (Input.GetKeyDown(KeyCode.G))
        {
            // Store the last position and rotation of the active camera
            if (currentCameraIndex < cameras.Count && cameras[currentCameraIndex] != null)
            {
                lastCameraPosition = cameras[currentCameraIndex].transform.position;
                lastCameraRotation = cameras[currentCameraIndex].transform.rotation;

                // Disable the currently enabled camera
                cameras[currentCameraIndex].enabled = false;
            }

            // Set god mode object position and rotation to match the last camera's position and rotation
            godModeObject.transform.position = lastCameraPosition;
            godModeObject.transform.rotation = lastCameraRotation;

            godModeObject.SetActive(true);
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
