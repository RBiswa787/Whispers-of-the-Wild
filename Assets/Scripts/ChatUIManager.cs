using UnityEngine;
using UnityEngine.UI; // Keep for Button, unless using TMP Buttons
using TMPro; // Required for TextMeshPro components
using System.Collections.Generic; // Required for List
using System.Collections; // Required for IEnumerator (if using ScrollToBottom)

/// <summary>
/// Manages the Chat UI logic, including display and input elements, text population, and button actions.
/// NOTE: This script NO LONGER controls the visibility (SetActive) of the displayPanel and inputPanel.
/// Visibility should be managed by an external script or system.
/// Handles displaying ONLY the last response using TextMeshPro.
/// </summary>
public class ChatUIManager : MonoBehaviour
{

     public static ChatUIManager Instance { get; private set; }
     
    [Header("UI References")]
    [Tooltip("The panel containing the chat display text. Visibility managed externally.")]
    public GameObject displayPanel; // Panel for showing messages
    [Tooltip("The panel containing the input field and send button. Visibility managed externally.")]
    public GameObject inputPanel; // Panel for user input
    [Tooltip("TextMeshPro element to display the conversation history (will only show the last response).")]
    public TextMeshProUGUI chatDisplay; // Changed to TextMeshProUGUI
    [Tooltip("TextMeshPro Input field for the player to type messages.")]
    public TMP_InputField chatInput; // Changed to TMP_InputField
    [Tooltip("Button to send the player's message.")]
    public Button sendButton; // Assumes standard UI Button
    [Tooltip("Button to close the chat panels (logic might need adjustment depending on external control).")]
    public Button closeButton; // Assumes standard UI Button

    // [Header("Chat Settings")] // No longer needed for simple last message display
    // [Tooltip("Maximum number of messages to keep in the display history.")]
    // public int maxHistory = 20;

    // Reference to the currently interacting NPC
    private NPCInteraction currentNPC;
    // Reference to the OpenAI handler
    public OpenAIChatHandler openAIHandler { get; private set; }

    // Store conversation history for context (still needed for OpenAI)
    private List<OpenAIChatHandler.ChatMessage> conversationHistory = new List<OpenAIChatHandler.ChatMessage>();


    void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            openAIHandler = FindObjectOfType<OpenAIChatHandler>();
            // Optional: Prevent the GameObject from being destroyed on scene load
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("Multiple ChatUIManager instances found! Destroying the extra one.");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Find the OpenAI Handler in the scene
        
        if (openAIHandler == null)
        {
            Debug.LogError("ChatUIManager could not find an OpenAIChatHandler in the scene!", this);
        }

        // Add listeners to buttons
        if (sendButton != null) sendButton.onClick.AddListener(OnSendButtonClicked);
        // The closeButton listener might need to call your external closing logic now
        if (closeButton != null) closeButton.onClick.AddListener(CloseChat);
        if (chatInput != null) chatInput.onSubmit.AddListener(OnInputSubmit); // Allow sending with Enter key

        // --- Visibility control removed ---
        // Ensure required references are assigned, but don't control SetActive
        if (displayPanel == null)
        {
            Debug.LogError("Display Panel is not assigned in the ChatUIManager inspector!", this);
        }
        if (inputPanel == null)
        {
             Debug.LogError("Input Panel is not assigned in the ChatUIManager inspector!", this);
        }
         if (chatDisplay == null)
        {
             Debug.LogError("Chat Display (TextMeshProUGUI) is not assigned in the ChatUIManager inspector!", this);
        }
         if (chatInput == null)
        {
             Debug.LogError("Chat Input (TMP_InputField) is not assigned in the ChatUIManager inspector!", this);
        }
    }

    /// <summary>
    /// Call this method when a chat should start (e.g., from NPCInteraction's UnityEvent).
    /// Initializes the chat state but DOES NOT activate the UI panels.
    /// </summary>
    /// <param name="npc">The NPCInteraction component that triggered the chat.</param>
    public void StartChat(NPCInteraction npc)
    {
        // Check essential references
        if (chatDisplay == null || chatInput == null || openAIHandler == null)
        {
            Debug.LogError("Chat UI elements (Display, Input) or OpenAI Handler not set up correctly in the Inspector.", this);
            return;
        }
         if (npc == null)
        {
             Debug.LogError("StartChat called with a null NPC.", this);
             return;
        }

        currentNPC = npc;
        conversationHistory.Clear(); // Start a fresh conversation
        chatDisplay.text = ""; // Clear previous text displayed

        // Add the initial system prompt to the history (but DO NOT display it)
        conversationHistory.Add(new OpenAIChatHandler.ChatMessage { role = "system", content = currentNPC.GetSystemPrompt() });

        // AddMessageToDisplay("System", $"Preparing chat with {currentNPC.gameObject.name}."); // REMOVED: Do not display system messages

        // --- Visibility control removed ---
        // displayPanel.SetActive(true);
        // inputPanel.SetActive(true);

        // Prepare input field
        chatInput.text = "";
        // Only activate input field if the panel is already visible (controlled externally)
        if(inputPanel != null && inputPanel.activeInHierarchy)
        {
            chatInput.ActivateInputField(); // Set focus to input field
        }

        // Optional: Pause game, disable player movement (keep if needed)
        // Time.timeScale = 0f;
    }

    /// <summary>
    /// Called when the Send button is clicked or Enter is pressed in the input field.
    /// </summary>
    private void OnSendButtonClicked()
    {
        if (chatInput == null || currentNPC == null || openAIHandler == null) return; // Safety check

        string playerMessage = chatInput.text;
        if (!string.IsNullOrWhiteSpace(playerMessage))
        {
            // Add player message to history (DO NOT display it)
            conversationHistory.Add(new OpenAIChatHandler.ChatMessage { role = "user", content = playerMessage });

            // Clear input field
            chatInput.text = "";
            chatInput.ActivateInputField(); // Keep focus

            // Disable input while waiting for response
            SetInputActive(false);

            // Send conversation to OpenAI
            openAIHandler.GetResponse(conversationHistory, OnOpenAIResponseReceived);
        }
         else
        {
             // Keep focus even if message is empty, allows easy correction
             chatInput.ActivateInputField();
        }
    }

    /// <summary>
    /// Called when the player presses Enter in the input field.
    /// </summary>
    /// <param name="text">The text submitted (passed by TMP_InputField.onSubmit).</param>
    private void OnInputSubmit(string text)
    {
        // TMP_InputField's onSubmit handles the Enter key press directly.
        // We just need to make sure the button logic runs.
        // Check if Enter was pressed (redundant check, but safe)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
             OnSendButtonClicked();
        }
    }

    /// <summary>
    /// Callback function executed when a response is received from OpenAI.
    /// </summary>
    /// <param name="success">Whether the API call was successful.</param>
    /// <param name="responseMessage">The content of the response or error message.</param>
    private void OnOpenAIResponseReceived(bool success, string responseMessage)
    {
        if (chatDisplay == null) return; // Safety check

        if (success)
        {
            // Add NPC response to history
            conversationHistory.Add(new OpenAIChatHandler.ChatMessage { role = "assistant", content = responseMessage });

            // Display ONLY the latest NPC response
            string npcName = (currentNPC != null) ? currentNPC.gameObject.name : "NPC";
            string displayMessage = $"<b>{npcName}:</b> {responseMessage}";
            DisplayLastMessage(displayMessage);
        }
        else
        {
            // Display the error message
            string displayMessage = $"<b>Error:</b> {responseMessage}";
            DisplayLastMessage(displayMessage);
        }

        // Re-enable input
        SetInputActive(true);
        // Only activate input field if the panel is still visible (controlled externally)
        if(chatInput != null && inputPanel != null && inputPanel.activeInHierarchy)
        {
             chatInput.ActivateInputField(); // Refocus after response
        }
    }

    /// <summary>
    /// Displays the given message in the chat display area, replacing previous content.
    /// </summary>
    /// <param name="messageToDisplay">The formatted message string to show.</param>
    private void DisplayLastMessage(string messageToDisplay)
    {
        if (chatDisplay != null)
        {
            // Replace the entire text content with the new message
            chatDisplay.text = messageToDisplay;

            // Auto-scrolling is not needed when only showing one line.
            // If the response is very long and wraps, you might still need this,
            // but for a single message view, it's likely unnecessary.
            // Consider calling this from your external script after activating the panel if needed.
            // Canvas.ForceUpdateCanvases();
            // ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
            // if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
            // StartCoroutine(ScrollToBottom()); // Use if message wrapping makes scrolling relevant
        }
    }

     /// <summary>
    /// Enables or disables the chat input field and send button.
    /// </summary>
    private void SetInputActive(bool isActive)
    {
       if(chatInput != null) chatInput.interactable = isActive;
       if(sendButton != null) sendButton.interactable = isActive;
    }

    /// <summary>
    /// Cleans up chat state. Called by the close button or potentially your external logic.
    /// DOES NOT deactivate the UI panels.
    /// </summary>
    public void CloseChat()
    {
        // --- Visibility control removed ---
        // if (displayPanel != null) displayPanel.SetActive(false);
        // if (inputPanel != null) inputPanel.SetActive(false);

        Debug.Log("CloseChat called: Clearing state."); // Log for debugging

        currentNPC = null; // Clear current NPC reference
        conversationHistory.Clear(); // Clear history for next time
        if(chatDisplay != null) chatDisplay.text = ""; // Clear display text

        // Optional: Unpause game, enable player movement (keep if needed)
        // Time.timeScale = 1f;

        // Note: The closeButton listener still calls this method.
        // You might want the closeButton to call your external script's
        // method that handles hiding the panels AND THEN calls this CloseChat()
        // method to clean up the state.
    }

    // Optional Coroutine for scrolling with TMP (only if long responses wrap)
    // private System.Collections.IEnumerator ScrollToBottom()
    // {
    //      yield return new WaitForEndOfFrame();
    //      ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
    //      if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    // }

    // Removed unused method: PruneHistory is not relevant for displaying only the last message
    // private void PruneHistory()
    // { ... }

    // Removed: AddMessageToHistoryAndDisplay is no longer used as history
    // adding and display are now separate logic paths based on message role.
    // private void AddMessageToHistoryAndDisplay(string role, string content)
    // { ... }

    // Renamed and modified AddMessageToDisplay to only display the last message
    // private void AddMessageToDisplay(string prefix, string message)
    // { ... }
}