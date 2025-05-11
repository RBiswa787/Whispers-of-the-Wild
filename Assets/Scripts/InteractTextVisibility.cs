using UnityEngine;
using TMPro; // Make sure to include this namespace for TextMeshPro

public class TMPTriggerVisibility : MonoBehaviour
{
    // Assign your TextMeshProUGUI object here in the Inspector
    public TextMeshProUGUI textMeshProElement;

    void Start()
    {
        // Ensure the TMP element is initially hidden when the scene starts
        if (textMeshProElement != null)
        {
            textMeshProElement.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI element is not assigned in the Inspector for " + gameObject.name);
        }

        // Optional: Check if the collider is set as a trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("Collider on " + gameObject.name + " is not set as a trigger. Trigger events will not fire.", this);
        }
        else if (col == null)
        {
             Debug.LogWarning("No Collider found on " + gameObject.name + ". Trigger events will not fire.", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the TMP element is currently active AND the TAB key is pressed
        if (textMeshProElement != null && textMeshProElement.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Tab))
        {
            // If both conditions are true, hide the TMP element
            textMeshProElement.gameObject.SetActive(false);
            Debug.Log("TAB key pressed. Hiding TMP.");
        }
    }

    // Called when another collider enters the trigger zone
    private void OnTriggerEnter(Collider other)
    {
        // Ignore collisions with objects tagged as "Terrain"
        if (other.CompareTag("Terrain"))
        {
            return; // Exit the function, effectively ignoring this collision
        }

        // Check if the TMP element is assigned before trying to show it
        // We only show it if it's not already active (prevents unnecessary calls)
        if (textMeshProElement != null && !textMeshProElement.gameObject.activeSelf)
        {
            textMeshProElement.gameObject.SetActive(true);
            Debug.Log("Collider entered trigger. Showing TMP.");
            Debug.Log(other);
        }
    }

    // Called when another collider exits the trigger zone
    private void OnTriggerExit(Collider other)
    {
        // Ignore collisions with objects tagged as "Terrain"
        if (other.CompareTag("Terrain"))
        {
            return; // Exit the function, effectively ignoring this collision
        }

        // Check if the TMP element is assigned before trying to hide it
        // We only hide it if it's currently active (prevents unnecessary calls)
        if (textMeshProElement != null && textMeshProElement.gameObject.activeSelf)
        {
            textMeshProElement.gameObject.SetActive(false);
            Debug.Log("Collider exited trigger. Hiding TMP.");
        }
    }

    // Optional: Called while another collider is inside the trigger zone
    // private void OnTriggerStay(Collider other)
    // {
    //     // You could add logic here if needed, but for simple show/hide, Enter/Exit is sufficient.
    // }
}