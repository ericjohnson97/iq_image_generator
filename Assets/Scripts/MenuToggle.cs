using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public GameObject menu; // Assign your menu GameObject in the inspector

    // Update is called once per frame
    void Update()
    {
        // Check if the Escape key was pressed this frame
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle the active state of the menu GameObject
            if (menu != null)
            {
                menu.SetActive(!menu.activeSelf);
            }
        }
    }
}
