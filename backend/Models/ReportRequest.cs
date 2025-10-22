namespace Backend.Models;

/// <summary>
/// Represents a financial report generation request
/// </summary>
public class ReportRequest
{
    public string? ReportType { get; set; }
    public string? ReportTypeName { get; set; }
    public int Year { get; set; }
    public string? ClientName { get; set; }
    public string? GeneratedDate { get; set; }
    public string? ReportId { get; set; }
}
