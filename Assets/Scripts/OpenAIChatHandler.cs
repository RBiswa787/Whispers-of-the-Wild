using UnityEngine;
using UnityEngine.Networking; // Required for UnityWebRequest
using System; // Required for Action
using System.Text; // Required for Encoding
using System.Collections; // Required for Coroutine
using System.Collections.Generic; // Required for List
using UnityEngine.Events;


// Define a UnityEvent that can pass a boolean value
    [System.Serializable]
    public class BooleanUnityEvent : UnityEvent<bool> {}
/// <summary>
/// Handles communication with the OpenAI Chat Completions API using UnityWebRequest.
/// </summary>
public class OpenAIChatHandler : MonoBehaviour
{
    [Header("OpenAI API Settings")]
    [Tooltip("Your OpenAI API Key. IMPORTANT: Do not hardcode in production!")]
    public string apiKey = "YOUR_OPENAI_API_KEY"; // <-- REPLACE THIS!
    [Tooltip("The OpenAI model to use for chat completions.")]
    public string model = "gpt-3.5-turbo"; // Or "gpt-4", etc.
    [Tooltip("The endpoint URL for OpenAI Chat Completions API.")]
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    
    // The UnityEvent that will be triggered
    public BooleanUnityEvent OnInteractionStateChanged;

    // Define simple classes to match the JSON structure for requests and responses
    // Based on OpenAI API documentation: https://platform.openai.com/docs/api-reference/chat/create

    [System.Serializable]
    public class ChatMessage
    {
        public string role; // "system", "user", or "assistant"
        public string content;
    }

    [System.Serializable]
    private class OpenAIRequest
    {
        public string model;
        public List<ChatMessage> messages;
        // Optional parameters (temperature, max_tokens, etc.) can be added here
        // public float temperature = 0.7f;
    }

    [System.Serializable]
    private class OpenAIResponseChoice
    {
        public int index;
        public ChatMessage message;
        public string finish_reason;
    }

    [System.Serializable]
    private class OpenAIResponse
    {
        public string id;
        public string @object; // Use @ to allow 'object' as variable name
        public long created;
        public string model;
        public List<OpenAIResponseChoice> choices;
        // Add usage statistics if needed
        // public Usage usage;
    }

    // [System.Serializable]
    // private class Usage
    // {
    //     public int prompt_tokens;
    //     public int completion_tokens;
    //     public int total_tokens;
    // }


    /// <summary>
    /// Sends the conversation history to the OpenAI API and invokes the callback with the response.
    /// </summary>
    /// <param name="messages">The list of ChatMessage objects representing the conversation.</param>
    /// <param name="callback">Action to call upon completion: (bool success, string responseOrError) => {}</param>
    public void GetResponse(List<ChatMessage> messages, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY")
        {
            callback?.Invoke(false, "OpenAI API Key is not set in OpenAIChatHandler.");
            return;
        }

        StartCoroutine(SendRequestToOpenAI(messages, callback));
    }

    private IEnumerator SendRequestToOpenAI(List<ChatMessage> messages, Action<bool, string> callback)
    {
        // 1. Construct the request payload
        OpenAIRequest requestPayload = new OpenAIRequest
        {
            model = this.model,
            messages = messages
            // Add any other parameters like temperature here if needed
        };
        string jsonPayload = JsonUtility.ToJson(requestPayload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        // 2. Create the UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 3. Set Headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            // 4. Send the request and wait for response
            yield return request.SendWebRequest();

            // 5. Process the response
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"OpenAI API Error: {request.error}\nResponse Code: {request.responseCode}\nResponse Body: {request.downloadHandler.text}");
                callback?.Invoke(false, $"API Error: {request.error} (Code: {request.responseCode})");
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("OpenAI Raw Response: " + jsonResponse); // Log raw response for debugging

                try
                {
                    // Parse the JSON response
                    OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(jsonResponse);

                    if (response != null && response.choices != null && response.choices.Count > 0)
                    {
                        // Get the message content from the first choice
                        string responseMessage = response.choices[0].message.content.Trim();
                        callback?.Invoke(true, responseMessage);

                        OnInteractionStateChanged?.Invoke(true);
                        Debug.Log("Talking animation triggered.");

                        
                    }
                    else
                    {
                        Debug.LogError("OpenAI Response Parsing Error: 'choices' array is null or empty.");
                        callback?.Invoke(false, "Failed to parse response from OpenAI.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse OpenAI JSON response: {e.Message}\nJSON: {jsonResponse}");
                    callback?.Invoke(false, "Error parsing OpenAI response.");
                }
            }
            else
            {
                 // Handle other potential request results if necessary
                 Debug.LogError($"OpenAI Request failed with result: {request.result}");
                 callback?.Invoke(false, $"Request failed: {request.result}");
            }
        } // using statement ensures request is disposed properly
    }
}
