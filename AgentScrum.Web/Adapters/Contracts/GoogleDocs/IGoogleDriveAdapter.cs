namespace AgentScrum.Web.Adapters.Contracts.GoogleDocs;

public interface IGoogleDriveAdapter
{
    string CreateDocument(string name, string markdownContent, string? shareWithEmail);
    bool HasValidCredentials();
    void SetCredentials(string credentialsJson);
    bool IsValidGoogleCredentialsJson(string jsonContent);
}