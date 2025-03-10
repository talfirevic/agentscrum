using System.Text.Json.Serialization;

namespace AgentScrum.Web.Adapters.Contracts.Groq;

/// <summary>
/// Represents a choice in the chat completion response.
/// </summary>
public class Choice
{
    /// <summary>
    /// Gets or sets the index of the choice.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the generated message.
    /// </summary>
    [JsonPropertyName("message")]
    public Message Message { get; set; }

    /// <summary>
    /// Gets or sets the reason the completion finished (e.g., "stop").
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}