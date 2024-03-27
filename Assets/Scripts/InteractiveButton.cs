using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class InteractiveButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public Image background; // The button's background image

    // Events that can be assigned from the Unity Editor or other scripts
    public UnityEvent onButtonSelected;
    public UnityEvent onButtonClicked;
    public UnityEvent onButtonDeselected;

    public Color selectedColor; 
    public Color deselectedColor;
    public Color clickedColor;

    private void Awake()
    {
        // Initialize the background if it's not manually assigned
        if (background == null)
        {
            background = GetComponent<Image>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        background.color = clickedColor; 
        onButtonClicked.Invoke(); // Invoke click event

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.color = selectedColor;
        onButtonSelected.Invoke(); // Invoke select (hover) event
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.color = deselectedColor;
        onButtonDeselected.Invoke(); // Invoke deselect event
    }

    // Additional methods can be added to provide more specific behaviors if needed
}
