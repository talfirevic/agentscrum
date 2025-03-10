using System.Text;
using AgentScrum.Web.Adapters.Contracts.GoogleDocs;
using Google.Apis.Drive.v3;
using Markdig;

namespace AgentScrum.Web.Adapters.Implementations;

/// <summary>
/// Adapter for creating new Google Docs from markdown content using the Google Drive API.
/// </summary>
public class GoogleDriveAdapter : IGoogleDriveAdapter
{
    private readonly DriveService _driveService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleDriveAdapter"/> class.
    /// </summary>
    /// <param name="driveService">An authenticated Google Drive service instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="driveService"/> is null.</exception>
    public GoogleDriveAdapter(DriveService driveService)
    {
        _driveService = driveService ?? throw new ArgumentNullException(nameof(driveService));
    }

    /// <summary>
    /// Creates a new Google Doc with the specified name and markdown content.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="markdownContent">The content of the document in markdown format.</param>
    /// <returns>The ID of the created Google Doc.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="markdownContent"/> is null.</exception>
    /// <exception cref="Google.GoogleApiException">Thrown when the API request fails.</exception>
    public string CreateDocument(string name, string markdownContent)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (markdownContent == null) throw new ArgumentNullException(nameof(markdownContent));

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
            request.Fields = "id"; // Request only the file ID in the response

            // Perform the upload synchronously
            request.Upload();

            // Return the ID of the created document
            return request.ResponseBody.Id;
        }
    }
}