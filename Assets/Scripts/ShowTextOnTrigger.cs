using TMPro;
using UnityEngine;

public class ShowTextOnTrigger : MonoBehaviour
{
    public TMP_Text textToControl; // Drag your TextMeshPro Text object here
    private bool isTextVisible = false;

    // Ensure the TextMeshPro component is assigned in the Inspector
    void OnValidate()
    {
        if (textToControl == null)
        {
            Debug.LogWarning("TextMeshPro Text object is not assigned in the Inspector!", this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initially hide the text
        if (textToControl != null)
        {
            textToControl.enabled = false;
        }
        else
        {
            Debug.LogError("TextMeshPro Text object is null. Please assign it in the Inspector!", this);
            enabled = false; // Disable the script if the text object is missing
        }
    }

    // Public method to show the text
    public void ShowText()
    {
        if (textToControl != null)
        {
            textToControl.enabled = true;
            isTextVisible = true;
        }
    }

    // Public method to hide the text
    public void HideText()
    {
        if (textToControl != null)
        {
            textToControl.enabled = false;
            isTextVisible = false;
        }
    }

    // Public method to toggle the visibility of the text
    public void ToggleTextVisibility()
    {
        if (textToControl != null)
        {
            isTextVisible = !isTextVisible;
            textToControl.enabled = isTextVisible;
        }
    }

    // Optional: You can also have a method to set the text before showing it
    public void SetAndShowText(string newText)
    {
        if (textToControl != null)
        {
            textToControl.text = newText;
            ShowText();
        }
    }
}