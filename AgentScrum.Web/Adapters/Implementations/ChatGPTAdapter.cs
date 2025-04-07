using System.Text;
using System.Text.Json;
using AgentScrum.Web.Adapters.Contracts;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AgentScrum.Web.Adapters.Implementations;

/// <summary>
/// Provides methods to interact with the OpenAI API's chat endpoint using Microsoft.Extensions.AI.
/// </summary>
public class ChatGPTAdapter : IChatGptAdapter
{
    private readonly IChatClient _chatClient;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatGPTAdapter"/> class.
    /// </summary>
    /// <param name="apiKey">The API key for authenticating requests to the OpenAI API.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiKey"/> is null or empty.</exception>
    public ChatGPTAdapter(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentNullException(nameof(apiKey), "API key cannot be null or empty.");

        // Create an OpenAI client and convert it to an IChatClient using Microsoft.Extensions.AI
        var openAIClient = new OpenAIClient(apiKey);
        _chatClient = openAIClient.AsChatClient();
    }

    /// <summary>
    /// Asynchronously creates a chat completion using the OpenAI API.
    /// </summary>
    /// <param name="request">The chat completion request containing messages and parameters.</param>
    /// <returns>A task that resolves to the chat completion response.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
    /// <exception cref="Exception">Thrown if the API returns an error response.</exception>
    public async Task<ChatCompletionResponse?> CreateChatCompletionAsync(ChatCompletionRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");

        try
        {
            // Convert our request model to Microsoft.Extensions.AI model
            var messages = request.Messages.Select(m => new Microsoft.Extensions.AI.ChatMessage(m.Role, m.Content)).ToList();
            
            // Set up chat options
            var chatOptions = new ChatOptions
            {
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                TopP = request.TopP,
                ModelId = request.Model
            };

            // Make the request using Microsoft.Extensions.AI
            var response = await _chatClient.GetResponseAsync(messages, chatOptions);

            // Convert the response back to our model
            return new ChatCompletionResponse
            {
                Id = Guid.NewGuid().ToString(), // OpenAI response doesn't directly map to our model
                Object = "chat.completion",
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = request.Model,
                Choices = new List<Choice>
                {
                    new Choice
                    {
                        Index = 0,
                        Message = new Message
                        {
                            Role = "assistant",
                            Content = response.Message
                        },
                        FinishReason = "stop" // Simplified
                    }
                },
                Usage = new Usage
                {
                    // Microsoft.Extensions.AI doesn't provide token usage info directly
                    // so we're setting default values
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    TotalTokens = 0
                }
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling OpenAI API: {ex.Message}", ex);
        }
    }
}