namespace AgentScrum.Web.Adapters.Contracts.Groq;

/// <summary>
/// Custom exception thrown when the Groq API returns an error.
/// </summary>
public class GroqApiException : Exception
{
    /// <summary>
    /// Gets the type of error returned by the API.
    /// </summary>
    public string ErrorType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroqApiException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorType">The type of error.</param>
    public GroqApiException(string message, string errorType) : base(message)
    {
        ErrorType = errorType;
    }
}