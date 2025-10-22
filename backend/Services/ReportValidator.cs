namespace Backend.Services;

using Backend.Models;

/// <summary>
/// Validates report generation requests to ensure all required data is present and valid
/// </summary>
public class ReportValidator
{
    private const int MinClientNameLength = 2;

    /// <summary>
    /// Checks if a report request contains all the necessary information
    /// </summary>
    public (bool IsValid, string? ErrorMessage) ValidateRequest(ReportRequest? request)
    {
        if (request == null)
        {
            return (false, "Invalid request data");
        }

        if (string.IsNullOrWhiteSpace(request.ClientName) || request.ClientName.Length < MinClientNameLength)
        {
            return (false, "Client name must be at least 2 characters");
        }

        if (string.IsNullOrWhiteSpace(request.ReportTypeName))
        {
            return (false, "Report type is required");
        }

        if (request.Year <= 0)
        {
            return (false, "Valid year is required");
        }

        return (true, null);
    }
}
