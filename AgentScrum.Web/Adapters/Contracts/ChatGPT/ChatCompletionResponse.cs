using System.Text.Json.Serialization;

namespace AgentScrum.Web.Adapters.Contracts;

/// <summary>
/// Represents the response from a chat completion request to ChatGPT.
/// </summary>
public class ChatCompletionResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the completion.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object type (e.g., "chat.completion").
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Unix timestamp of when the completion was created.
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }

    /// <summary>
    /// Gets or sets the model used for the completion.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of completion choices.
    /// </summary>
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();

    /// <summary>
    /// Gets or sets the token usage statistics.
    /// </summary>
    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = new();
} 