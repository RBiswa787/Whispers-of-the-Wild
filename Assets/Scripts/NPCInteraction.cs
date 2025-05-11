using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent

/// <summary>
/// Attached to an NPC GameObject. Handles player proximity detection
/// and stores NPC-specific information like personality for the chat.
/// Requires a Collider component set to IsTrigger = true on the same GameObject.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Animator))]
public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Configuration")]
    [TextArea(5, 15)]
    [Tooltip("System message defining the NPC's personality, role, and knowledge.")]
    public string npcSystemPrompt = "You are a helpful shopkeeper named Bob. You are friendly, slightly grumpy, and know about the items in your shop. You keep your responses concise.";
    
    private ChatUIManager ChatUIManager;

    [Header("Interaction Settings")]
    [Tooltip("The key the player needs to press to initiate chat when nearby.")]
    public KeyCode interactionKey = KeyCode.E;
    [Tooltip("Tag of the Player GameObject.")]
    public string playerTag = "Player";

    [Header("Events")]
    [Tooltip("Event fired when the player interacts with this NPC.")]
    public UnityEvent<NPCInteraction> OnInteract; // Event to notify the ChatUIManager

    private bool isPlayerNearby = false;
    private GameObject playerObject = null;

    // Expose the Animator field to the Unity Inspector
    [SerializeField]
    private Animator _animator;
    private readonly int _isInteractingHash = Animator.StringToHash("random_int"); // More efficient

    void Awake()
    {
        // Ensure the collider is set to be a trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError($"NPC '{gameObject.name}' is missing a Collider component for interaction.", this);
        }

        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("NPCInteraction script requires an Animator component on the same GameObject.", this);
            enabled = false; 
            return;
        }


        ChatUIManager = FindObjectOfType<ChatUIManager>();
        Debug.Log(ChatUIManager);
        // Subscribe to the event from OpenAIChatHandler via ChatUIManager
        // Ensure ChatUIManager and OpenAIChatHandler instances are ready
       
    }

    void Start()
    {
        // Attempt to subscribe to the event in Start() for better reliability.
        if (ChatUIManager.Instance != null)
        {
            if (ChatUIManager.Instance.openAIHandler != null)
            {
                // It's also a good idea to check if the event itself is null before subscribing
                if (ChatUIManager.Instance.openAIHandler.OnInteractionStateChanged != null)
                {
                    ChatUIManager.Instance.openAIHandler.OnInteractionStateChanged.AddListener(HandleInteractionStateChanged);
                    Debug.Log($"{gameObject.name} successfully subscribed to OnInteractionStateChanged in Start.");
                }
                else
                {
                    Debug.LogError($"{gameObject.name}: OnInteractionStateChanged event is null in OpenAIChatHandler. Cannot subscribe.", this);
                }
            }
            else
            {
                Debug.LogError($"{gameObject.name}: ChatUIManager.Instance.openAIHandler is null in Start. Cannot subscribe.", this);
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}: ChatUIManager.Instance is null in Start. Cannot subscribe. Ensure ChatUIManager is in the scene and initializes its Instance correctly in Awake.", this);
        }
    }

    void Update()
    {
        // Check for interaction key press only if the player is nearby
        if (isPlayerNearby && Input.GetKeyDown(interactionKey))
        {
            StartInteraction();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the GameObject entering the trigger is the player
        if (other.CompareTag(playerTag))
        {
            isPlayerNearby = true;
            playerObject = other.gameObject;
            Debug.Log($"Player entered interaction range for {gameObject.name}. Press {interactionKey} to talk.");
            // Optional: Show an interaction prompt UI element here
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the GameObject leaving the trigger is the player
        if (other.CompareTag(playerTag))
        {
            isPlayerNearby = false;
            playerObject = null;
            Debug.Log($"Player exited interaction range for {gameObject.name}.");
            // Optional: Hide the interaction prompt UI element here
        }
    }

    /// <summary>
    /// Called when the player presses the interaction key while nearby.
    /// </summary>
    private void StartInteraction()
    {
        Debug.Log($"Interaction started with {gameObject.name}.");
        // Invoke the event, passing this NPC's interaction component
        // The ChatUIManager should listen to this event.
        OnInteract?.Invoke(this);
    }

    /// <summary>
    /// Public accessor for the NPC's system prompt.
    /// </summary>
    public string GetSystemPrompt()
    {
        return npcSystemPrompt;
    }

     // This method will be called when OpenAIChatHandler's event is invoked
    public void HandleInteractionStateChanged(bool newState)
    {
        Debug.Log("Handler");
        if (_animator != null)
        {
            Debug.Log("true");
            int randomValue = UnityEngine.Random.Range(1, 3); // Generates either 1 or 2
            Debug.Log($"{gameObject.name} received HandleInteractionStateChanged: {newState}. Updating Animator with random value: {randomValue}");
            _animator.SetInteger(_isInteractingHash, randomValue); // Assuming _isInteractingHash is an integer hash
        }
        else
        {
            Debug.Log("false");
            Debug.LogWarning($"{gameObject.name} received HandleInteractionStateChanged but Animator is null.", this);
        }
    }
}
