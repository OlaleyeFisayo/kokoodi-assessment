namespace Backend.Services;

/// <summary>
/// Handles the creation of safe, descriptive filenames for downloaded reports
/// </summary>
public class FileNameService
{
    /// <summary>
    /// Creates a unique filename for a report that's safe for any file system
    /// </summary>
    public string GenerateFileName(string clientName, int year)
    {
        string safeClientName = SanitizeFileName(clientName);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        return $"Report_{safeClientName}_{year}_{timestamp}.docx";
    }

    /// <summary>
    /// Strips out characters that aren't allowed in filenames
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();

        return string.Join("_", fileName.Split(invalidChars));
    }
}

