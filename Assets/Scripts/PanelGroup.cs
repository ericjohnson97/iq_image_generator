using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelGroup : MonoBehaviour
{
    public GameObject[] panels;
    public TabGroup tabGroup;

    public int panelIndex = 0;
    
    private void Awake()
    {
        ShowCurrentPanel();
    }

    public void ShowCurrentPanel()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panelIndex)
            {
                panels[i].SetActive(true);
            }
            else
            {
                panels[i].SetActive(false);
            }

        }
        
    }
    public void SetPageIndex(int index)
    {
        panelIndex = index;
        ShowCurrentPanel();
    }
}
