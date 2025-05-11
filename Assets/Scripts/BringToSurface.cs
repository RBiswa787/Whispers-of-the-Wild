using UnityEngine;

public class BringToSurface : MonoBehaviour
{
    public float targetHeightAboveStart = 2f; // Adjust the desired height in the Inspector
    public float liftSpeed = 5f;             // Adjust the speed of movement

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;
    public AudioSource audioSource;
    public ShowTextOnTrigger textController;

    void Start()
    {


        // Make sure an AudioSource exists
        if (audioSource == null)
        {
            Debug.LogError("AudioSource not found on this GameObject!");
        }
        audioSource.Stop();

        startPosition = transform.position;
        targetPosition = startPosition + Vector3.up * targetHeightAboveStart;
    }

    // Call this function from another script to start the upward movement
    public void StartLifting()
    {
        if (audioSource != null)
        {
            Debug.Log("Audio");
            audioSource.Play();
        }
        
        isMoving = true;
    }

    void Update()
    {
        if (isMoving)
        {
            // Move the object towards the target height
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, liftSpeed * Time.deltaTime);

            // Check if the object has reached the target height
            if (Mathf.Abs(transform.position.y - targetPosition.y) < 0.01f)
            {
                isMoving = false;
                audioSource.Stop();
                if (textController != null)
                {
                    textController.ShowText();
                }
            }
        }
    }
}