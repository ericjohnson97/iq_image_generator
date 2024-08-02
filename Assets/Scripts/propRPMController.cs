using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class propRPMController : MonoBehaviour
{
    public JSBUDPReceiver JSBRecv;
    public int RPMIndex;
    public float RPMScaleFactor = 10f;
    public int direction = 1;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (RPMIndex >= 0 && RPMIndex < JSBRecv.AircraftState.rpm.Length)
        // {
        //     float rpmValue = JSBRecv.AircraftState.rpm[RPMIndex];
        //     float deltaRotation = rpmValue * direction * RPMScaleFactor;

        //     // Update the rotation
        //     this.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + deltaRotation, 0);
        // }
        // else
        // {
        //     Debug.LogError("RPMIndex is out of bounds.");
        // }

        // For testing without actual JSBRecv data
        this.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 100 * (float)direction * RPMScaleFactor, 0);
    }
}
