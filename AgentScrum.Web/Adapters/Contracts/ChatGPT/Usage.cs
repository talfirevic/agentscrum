using System.Text.Json.Serialization;

namespace AgentScrum.Web.Adapters.Contracts;

/// <summary>
/// Represents token usage statistics in the ChatGPT response.
/// </summary>
public class Usage
{
    /// <summary>
    /// Gets or sets the number of tokens used for the prompt.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens used for the completion.
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Gets or sets the total number of tokens used.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
} 