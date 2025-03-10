using System.Text.Json.Serialization;

namespace AgentScrum.Web.Adapters.Contracts.Groq;

/// <summary>
/// Represents token usage statistics in the response.
/// </summary>
public class Usage
{
    /// <summary>
    /// Gets or sets the number of prompt tokens used.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of completion tokens generated.
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Gets or sets the total number of tokens used.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}