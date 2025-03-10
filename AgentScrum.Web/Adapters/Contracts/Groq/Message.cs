using System.Text.Json.Serialization;

namespace AgentScrum.Web.Adapters.Contracts.Groq;

/// <summary>
/// Represents a message in the chat conversation.
/// </summary>
public class Message
{
    /// <summary>
    /// Gets or sets the role of the message sender. Must be "system", "user", or "assistant".
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; }

    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }
}