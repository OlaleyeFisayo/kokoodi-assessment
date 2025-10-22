using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy to allow frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// POST endpoint to generate and download report
app.MapPost("/api/reports/generate", async (HttpContext context) =>
{
    try
    {
        var requestData = await JsonSerializer.DeserializeAsync<ReportRequest>(
            context.Request.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (requestData == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid request data" });
            return;
        }

        if (string.IsNullOrWhiteSpace(requestData.ClientName) || requestData.ClientName.Length < 2)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = "Client name must be at least 2 characters" });
            return;
        }

        if (string.IsNullOrWhiteSpace(requestData.ReportTypeName))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = "Report type is required" });
            return;
        }

        if (requestData.Year <= 0)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = "Valid year is required" });
            return;
        }

        using var memoryStream = new MemoryStream();
        CreateReportDocument(memoryStream, requestData.ClientName, requestData.ReportTypeName, requestData.Year, requestData.ReportId ?? "N/A");

        memoryStream.Position = 0;

        string safeClientName = string.Join("_", requestData.ClientName.Split(Path.GetInvalidFileNameChars()));
        string fileName = $"Report_{safeClientName}_{requestData.Year}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";

        context.Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        context.Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";

        await memoryStream.CopyToAsync(context.Response.Body);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { message = "Error generating report", error = ex.Message });
    }
});

// Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

Console.WriteLine("Server running on: http://localhost:5000");
Console.WriteLine("API Endpoint: POST http://localhost:5000/api/reports/generate");
app.Run("http://localhost:5000");

// Method to create Word document in memory
static void CreateReportDocument(Stream stream, string clientName, string reportType, int year, string reportId)
{
    using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
    {
        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
        mainPart.Document = new Document();
        Body body = mainPart.Document.AppendChild(new Body());

        // Main report title
        Paragraph titlePara = body.AppendChild(new Paragraph());
        Run titleRun = titlePara.AppendChild(new Run());

        RunProperties titleProps = titleRun.AppendChild(new RunProperties());
        titleProps.AppendChild(new Bold());
        titleProps.AppendChild(new FontSize() { Val = "32" });
        titleProps.AppendChild(new Color() { Val = "1a1a1a" });

        titleRun.AppendChild(new Text($"{reportType} Report"));

        // Client name paragraph
        Paragraph clientPara = body.AppendChild(new Paragraph());
        Run clientRun = clientPara.AppendChild(new Run());

        RunProperties clientProps = clientRun.AppendChild(new RunProperties());
        clientProps.AppendChild(new FontSize() { Val = "24" });

        clientRun.AppendChild(new Text($"Client: {clientName}"));

        // Year paragraph
        Paragraph yearPara = body.AppendChild(new Paragraph());
        Run yearRun = yearPara.AppendChild(new Run());

        RunProperties yearProps = yearRun.AppendChild(new RunProperties());
        yearProps.AppendChild(new FontSize() { Val = "24" });

        yearRun.AppendChild(new Text($"Reporting Year: {year}"));

        // Add spacing
        body.AppendChild(new Paragraph());

        // Metadata paragraph
        Paragraph metaPara = body.AppendChild(new Paragraph());
        Run metaRun = metaPara.AppendChild(new Run());

        RunProperties metaRunProps = metaRun.AppendChild(new RunProperties());
        metaRunProps.AppendChild(new Italic());
        metaRunProps.AppendChild(new FontSize() { Val = "20" });
        metaRunProps.AppendChild(new Color() { Val = "737373" });

        string metadata = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\nReport ID: {reportId}";
        metaRun.AppendChild(new Text(metadata) { Space = SpaceProcessingModeValues.Preserve });

        // Add sample content section
        body.AppendChild(new Paragraph());

        Paragraph contentHeaderPara = body.AppendChild(new Paragraph());
        Run contentHeaderRun = contentHeaderPara.AppendChild(new Run());

        RunProperties contentHeaderProps = contentHeaderRun.AppendChild(new RunProperties());
        contentHeaderProps.AppendChild(new Bold());
        contentHeaderProps.AppendChild(new FontSize() { Val = "24" });

        contentHeaderRun.AppendChild(new Text("Financial Summary"));

        // Sample content
        Paragraph contentPara = body.AppendChild(new Paragraph());
        Run contentRun = contentPara.AppendChild(new Run());
        contentRun.AppendChild(new Text("This is a sample financial report generated using the OpenXML SDK. In a production environment, this section would contain actual financial data, tables, charts, and detailed analysis based on the client's data."));

        mainPart.Document.Save();
    }
}

// Request model
public class ReportRequest
{
    public string? ReportType { get; set; }
    public string? ReportTypeName { get; set; }
    public int Year { get; set; }
    public string? ClientName { get; set; }
    public string? GeneratedDate { get; set; }
    public string? ReportId { get; set; }
}
