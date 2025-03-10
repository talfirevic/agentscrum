using System.Text.Json.Serialization;

namespace AgentScrum.Web.Adapters.Contracts.Groq;

/// <summary>
/// Represents the request payload for a chat completion.
/// </summary>
public class ChatCompletionRequest
{
    /// <summary>
    /// Gets or sets the list of messages in the conversation.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; }

    /// <summary>
    /// Gets or sets the model to use for the completion (e.g., "mixtral-8x7b-32768").
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; }

    /// <summary>
    /// Gets or sets the sampling temperature (optional, between 0 and 2).
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate (optional).
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Gets or sets the top-p probability mass (optional, between 0 and 1).
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>
    /// Gets or sets whether to stream the response (optional, defaults to false).
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// Gets or sets a sequence where the API will stop generating tokens (optional).
    /// </summary>
    [JsonPropertyName("stop")]
    public string Stop { get; set; }
}