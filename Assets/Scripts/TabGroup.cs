using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public List<TabBtn> tabButtons; // List to hold all tab buttons
    // Using Color instead of Sprite for button states
    public Color tabIdle;
    public Color tabHover;
    public Color tabActive;

    public TabBtn selectedTab; 
    public List<GameObject> objectsToSwap; 

    public PanelGroup panelGroup;


    /// <summary>
    /// Subscribes a new button to the tab group.
    /// </summary>
    /// <param name="button">The button to subscribe.</param>
    public void Subscribe(TabBtn button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabBtn>();
        }

        tabButtons.Add(button);
    }

    /// <summary>
    /// Handles mouse entering a tab button area by changing its color to hover.
    /// </summary>
    /// <param name="button">The button being hovered.</param>
    public void OnTabEnter(TabBtn button)
    {
        ResetTabs();
        if (button != null && button.background != null && selectedTab != button)
        {
            button.background.color = tabHover; // Set button color to hover color
        }
    }

    /// <summary>
    /// Handles mouse exiting a tab button area by reverting its color.
    /// </summary>
    /// <param name="button">The button being exited.</param>
    public void OnTabExit(TabBtn button)
    {
        ResetTabs();
        if (button != null && button.background != null && selectedTab != button)
        {
            button.background.color = tabIdle; // Set button color back to idle color
        }
    }

    /// <summary>
    /// Handles a tab button being selected, changing its color to active.
    /// </summary>
    /// <param name="button">The button being selected.</param>
    public void OnTabSelected(TabBtn button)
    {
        if (selectedTab != null)
        {
            selectedTab.Deselect();
        }
        selectedTab = button;
        selectedTab.Select();
        ResetTabs();
        if (button != null && button.background != null)
        {
            button.background.color = tabActive; // Set button color to active color
        }
        if (panelGroup != null)
        {
            panelGroup.SetPageIndex(tabButtons.IndexOf(selectedTab));
        }

    }

    /// <summary>
    /// Resets all tab buttons to their idle color.
    /// </summary>
    public void ResetTabs()
    {
        foreach (TabBtn button in tabButtons)
        {
            if (button != null && button.background != null && selectedTab != button)
            {
                button.background.color = tabIdle; // Reset button color to idle color
            }
        }
    }

    public void Start()
    {
        OnTabSelected(tabButtons[0]);
    }
}
