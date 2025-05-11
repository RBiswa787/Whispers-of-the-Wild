using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections; // Add this for Coroutines

public class ActivateLiftOnCollision : MonoBehaviour
{
    public BringToSurface objectToLift; // Drag your SubmergedObject here
    public string[] allowedCollisionTags = new string[5]; // Define the 5 allowed tags
    public GameObject[] linkedStatues = new GameObject[5]; // Drag the corresponding 5 statues here
    public string[] requiredCollisionSequence = new string[3]; // The specific sequence to activate the lift
    private List<string> currentCollisionSequence = new List<string>();
    public string terrainTag = "Terrain"; // Assign the tag of your Terrain GameObject
    private Dictionary<GameObject, Vector3> initialStatuePositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<string, GameObject> tagToStatue = new Dictionary<string, GameObject>();
    private List<GameObject> liftedStatues = new List<GameObject>();

    public float statueLiftSpeed = 0.2f; //  Speed at which statues are lifted *and lowered*.
    public float statueLiftAmount = 0.2f;
    public GameObject resetObject; // New GameObject to reset the sequence
    public string resetTag = "ResetObject"; //Tag for the reset object.

    public float resetSpeed = 1.0f; // Add this for controlling the speed of the reset.  Now used if statueLiftSpeed is zero.
    private bool isResetting = false; // Add this to prevent multiple resets
    private bool isLiftingStatue = false;
    private bool liftActivated = false; // Flag to track if the lift has been activated
    public AudioSource rockAudio;

    private void Start()
    {
        rockAudio.Stop();
        // Ensure the arrays have the same length for proper linking
        if (allowedCollisionTags.Length != linkedStatues.Length)
        {
            Debug.LogError("Allowed Collision Tags and Linked Statues arrays must have the same length in the Inspector of " + gameObject.name);
            enabled = false;
            return;
        }

        // Store the initial positions of the linked statues and create a tag-to-statue mapping
        for (int i = 0; i < allowedCollisionTags.Length; i++)
        {
            if (!string.IsNullOrEmpty(allowedCollisionTags[i]) && linkedStatues[i] != null)
            {
                initialStatuePositions.Add(linkedStatues[i], linkedStatues[i].transform.position);
                tagToStatue.Add(allowedCollisionTags[i], linkedStatues[i]);
            }
            else
            {
                Debug.LogError("Ensure all Allowed Collision Tags and Linked Statues are properly assigned in the Inspector of " + gameObject.name);
                enabled = false;
                return;
            }
        }

        if (requiredCollisionSequence.Length != 3)
        {
            Debug.LogWarning("Required Collision Sequence should have a length of 3 in the Inspector of " + gameObject.name);
        }

        //Check if the reset object is assigned
        if (resetObject == null)
        {
            Debug.LogError("Reset Object is not assigned in the Inspector of " + gameObject.name);
            enabled = false;
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (liftActivated)
          {
            return;
          }

        // Ignore collisions with Terrain
        if (collision.gameObject.CompareTag(terrainTag))
        {
            return;
        }

        string collidedTag = collision.gameObject.tag;

        if (collision.gameObject == resetObject || collidedTag == resetTag) //check for reset object or tag
        {
            Debug.Log("Reset collision detected. Resetting.");
            if (!isResetting) //prevent multiple resets
            {
                StartCoroutine(ResetSequence()); // Start the coroutine for slow reset
            }
            return; // Important: Exit the function after resetting!
        }

        // If the collided tag is one of the allowed tags, lift the corresponding statue
        if (tagToStatue.ContainsKey(collidedTag))
        {
            GameObject statueToLift = tagToStatue[collidedTag];
            if (statueToLift != null && !liftedStatues.Contains(statueToLift) && !isLiftingStatue) // Added isLiftingStatue check
            {
                //statueToLift.transform.Translate(Vector3.down * statueLiftAmount, Space.World);
                //liftedStatues.Add(statueToLift);
                StartCoroutine(LiftStatue(statueToLift)); // Start Coroutine for lifting
                liftedStatues.Add(statueToLift);
            }

            // Check if this collision is part of the required sequence
            if (currentCollisionSequence.Count < requiredCollisionSequence.Length && collidedTag == requiredCollisionSequence[currentCollisionSequence.Count])
            {
                currentCollisionSequence.Add(collidedTag);
                Debug.Log("Correct sequence collision: " + collidedTag + ". Current sequence: " + string.Join(", ", currentCollisionSequence));

                if (currentCollisionSequence.Count >= requiredCollisionSequence.Length)
                {
                    objectToLift.StartLifting();
                    enabled = false;
                    liftActivated = true; // Set the flag to disable further interactions
                    Debug.Log("Lift activated!");
                }
            }
            else if (!requiredCollisionSequence.Contains(collidedTag) || (currentCollisionSequence.Count > 0 && collidedTag != requiredCollisionSequence[currentCollisionSequence.Count]))
            {
                Debug.Log("Wrong sequence or unexpected collision: " + collidedTag + ". Resetting.");
                //StartCoroutine(ResetSequence());
            }
        }
        else
        {
            Debug.Log("Collided with an unallowed tag: " + collidedTag);
        }
    }

    private IEnumerator ResetSequence() // Changed to a Coroutine
    {
        isResetting = true; // Set the flag
        rockAudio.Play();
        currentCollisionSequence.Clear();
        // Lower any statues that have been lifted, over time
        foreach (GameObject statue in liftedStatues)
        {
            if (statue != null && initialStatuePositions.ContainsKey(statue))
            {
                Vector3 targetPosition = initialStatuePositions[statue];
                float distance = Vector3.Distance(statue.transform.position, targetPosition);
                float currentSpeed = (statueLiftSpeed > 0) ? statueLiftSpeed : resetSpeed; // Use statueLiftSpeed if it's greater than 0, otherwise use resetSpeed.

                while (distance > 0.01f) // Use a small threshold
                {
                    statue.transform.position = Vector3.MoveTowards(statue.transform.position, targetPosition, currentSpeed * Time.deltaTime);
                    yield return null; // Wait for the next frame
                    distance = Vector3.Distance(statue.transform.position, targetPosition);
                }
                statue.transform.position = targetPosition; // Ensure it reaches the exact position
            }
        }
        liftedStatues.Clear();
        isResetting = false; // Reset the flag
    }

    private IEnumerator LiftStatue(GameObject statueToLift)
    {
        rockAudio.Play();
        isLiftingStatue = true;
        Vector3 targetPosition = statueToLift.transform.position + Vector3.down * statueLiftAmount;
        float distance = Vector3.Distance(statueToLift.transform.position, targetPosition);
        while (distance > 0.01f)
        {
            statueToLift.transform.position = Vector3.MoveTowards(statueToLift.transform.position, targetPosition, statueLiftSpeed * Time.deltaTime);
            yield return null;
            distance = Vector3.Distance(statueToLift.transform.position, targetPosition);
        }
        statueToLift.transform.position = targetPosition;
        isLiftingStatue = false;
    }
}
