using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraListController : MonoBehaviour
{
    public GameObject TextTempate;
    public void CreateEntry( GameObject drone)
    {
        // TODO: create a better way to find the cameras
        Camera camera = drone.transform.Find("DynamicCameraTemplate").GetComponent<Camera>();
        if (camera != null)
        {
            GameObject text = Instantiate(TextTempate, transform);
            text.SetActive(true);
            text.GetComponent<TextMeshProUGUI>().text = $"Drone {drone.GetComponent<DroneController>().systemId}";
        }else
        {
            Debug.Log("Camera not found");
        }
    }
}
