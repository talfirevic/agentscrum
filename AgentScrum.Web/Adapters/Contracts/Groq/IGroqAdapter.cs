namespace AgentScrum.Web.Adapters.Contracts.Groq;

public interface IGroqAdapter
{
    Task<ChatCompletionResponse?> CreateChatCompletionAsync(ChatCompletionRequest request);
}