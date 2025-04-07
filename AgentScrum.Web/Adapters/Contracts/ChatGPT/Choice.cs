using System.Text.Json.Serialization;

namespace AgentScrum.Web.Adapters.Contracts;

/// <summary>
/// Represents a completion choice in the ChatGPT response.
/// </summary>
public class Choice
{
    /// <summary>
    /// Gets or sets the index of the choice.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the message containing the completion.
    /// </summary>
    [JsonPropertyName("message")]
    public Message Message { get; set; } = new();

    /// <summary>
    /// Gets or sets the reason why the completion stopped.
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
} 