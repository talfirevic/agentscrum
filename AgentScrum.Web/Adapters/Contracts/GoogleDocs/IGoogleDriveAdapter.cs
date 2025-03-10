namespace AgentScrum.Web.Adapters.Contracts.GoogleDocs;

public interface IGoogleDriveAdapter
{
    string CreateDocument(string name, string markdownContent);
}