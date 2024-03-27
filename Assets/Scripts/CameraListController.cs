using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraListController : MonoBehaviour
{
    public GameObject TextTempate;
    public void CreateEntry( GameObject drone)
    {
        Camera camera = drone.transform.Find("DynamicCamera").GetComponent<Camera>();
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
