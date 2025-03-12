using System.Text;
using System.Text.Json;
using AgentScrum.Web.Adapters.Contracts.GoogleDocs;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Markdig;

namespace AgentScrum.Web.Adapters.Implementations;

/// <summary>
/// Adapter for creating new Google Docs from markdown content using the Google Drive API.
/// </summary>
public class GoogleDriveAdapter : IGoogleDriveAdapter
{
    private readonly Func<string, DriveService> _driveServiceFactory;
    private DriveService? _driveService;
    
    /// <summary>
    /// Base URL for Google Docs documents.
    /// </summary>
    private const string GoogleDocsBaseUrl = "https://docs.google.com/document/d/{0}/edit";

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleDriveAdapter"/> class.
    /// </summary>
    /// <param name="driveService">An authenticated Google Drive service instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="driveService"/> is null.</exception>
    public GoogleDriveAdapter(DriveService driveService)
    {
        _driveService = driveService ?? throw new ArgumentNullException(nameof(driveService));
        _driveServiceFactory = _ => _driveService;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleDriveAdapter"/> class with a factory function.
    /// </summary>
    /// <param name="driveServiceFactory">A factory function that creates a DriveService from credentials JSON.</param>
    public GoogleDriveAdapter(Func<string, DriveService> driveServiceFactory)
    {
        _driveServiceFactory = driveServiceFactory ?? throw new ArgumentNullException(nameof(driveServiceFactory));
    }
    
    /// <summary>
    /// Sets the credentials JSON to use for creating the DriveService.
    /// </summary>
    /// <param name="credentialsJson">The Google service account credentials JSON.</param>
    public void SetCredentials(string credentialsJson)
    {
        if (string.IsNullOrEmpty(credentialsJson))
        {
            throw new ArgumentException("Credentials JSON cannot be null or empty.", nameof(credentialsJson));
        }
        
        _driveService = CreateDriveService(credentialsJson);
    }
    
    /// <summary>
    /// Creates a DriveService from credentials JSON.
    /// </summary>
    /// <param name="credentialsJson">The Google service account credentials JSON.</param>
    /// <returns>A configured DriveService.</returns>
    private DriveService CreateDriveService(string credentialsJson)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(credentialsJson));
        var credential = GoogleCredential.FromStream(stream)
            .CreateScoped(DriveService.Scope.Drive);
            
        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "AgentScrum API adapter"
        });
    }

    /// <summary>
    /// Creates a new Google Doc with the specified name and markdown content and shares it with a specified email.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="markdownContent">The content of the document in markdown format.</param>
    /// <param name="shareWithEmail">The email address to share the document with.</param>
    /// <returns>A JSON string containing the full response data of the created Google Doc, including a share link.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="markdownContent"/> is null.</exception>
    /// <exception cref="Google.GoogleApiException">Thrown when the API request fails.</exception>
    public string CreateDocument(string name, string markdownContent, string? shareWithEmail = null)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (markdownContent == null) throw new ArgumentNullException(nameof(markdownContent));
        
        // Ensure we have a DriveService
        if (_driveService == null)
        {
            throw new InvalidOperationException("DriveService is not initialized. Call SetCredentials first or use the constructor with a DriveService.");
        }

        // Convert markdown to HTML using Markdig
        string htmlContent = Markdown.ToHtml(markdownContent);

        // Convert HTML string to a memory stream for uploading
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent)))
        {
            // Define file metadata for the new Google Doc
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = name,
                MimeType = "application/vnd.google-apps.document"
            };

            // Create an upload request, converting HTML to Google Doc format
            var request = _driveService.Files.Create(fileMetadata, stream, "text/html");
            // Request all fields in the response
            request.Fields = "*";

            // Perform the upload synchronously
            request.Upload();
            
            var fileId = request.ResponseBody.Id;
            
            // Share the document if an email is provided
            if (!string.IsNullOrEmpty(shareWithEmail))
            {
                ShareDocument(fileId, shareWithEmail);
            }

            // Create a response object with the document data and share link
            var response = new
            {
                Document = request.ResponseBody,
                ShareLink = GetDocumentShareLink(fileId)
            };

            // Serialize the response to JSON and return it
            return JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
    
    /// <summary>
    /// Gets a shareable link for a Google Document.
    /// </summary>
    /// <param name="fileId">The ID of the document.</param>
    /// <returns>A URL that can be used to access the document.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileId"/> is null.</exception>
    private string GetDocumentShareLink(string fileId)
    {
        if (fileId == null) throw new ArgumentNullException(nameof(fileId));
        
        return string.Format(GoogleDocsBaseUrl, fileId);
    }
    
    /// <summary>
    /// Shares a Google Drive file with the specified email address.
    /// </summary>
    /// <param name="fileId">The ID of the file to share.</param>
    /// <param name="email">The email address to share the file with.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileId"/> or <paramref name="email"/> is null.</exception>
    /// <exception cref="Google.GoogleApiException">Thrown when the API request fails.</exception>
    private void ShareDocument(string fileId, string? email)
    {
        if (fileId == null) throw new ArgumentNullException(nameof(fileId));
        if (email == null) throw new ArgumentNullException(nameof(email));
        
        if (_driveService == null)
        {
            throw new InvalidOperationException("DriveService is not initialized.");
        }

        // Create a new permission
        var permission = new Google.Apis.Drive.v3.Data.Permission
        {
            Type = "user",
            Role = "writer",
            EmailAddress = email
        };

        // Create the permission request
        var request = _driveService.Permissions.Create(permission, fileId);
        
        // Set notification options (optional - set to false to avoid sending email notifications)
        request.SendNotificationEmail = true;
        
        // Execute the request
        request.Execute();
    }
}