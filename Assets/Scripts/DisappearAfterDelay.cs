using UnityEngine;

public class DisappearAfterDelayUpdate : MonoBehaviour
{
    public float delayInSeconds = 5f;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - startTime >= delayInSeconds)
        {
            Destroy(gameObject);
            // Optionally, you can disable the script once the object is destroyed
            enabled = false;
        }
    }
}