using Backend.Models;
using Backend.Services;
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

// Register services
builder.Services.AddSingleton<ReportValidator>();
builder.Services.AddSingleton<DocumentGenerator>();
builder.Services.AddSingleton<FileNameService>();

var app = builder.Build();

app.UseCors("AllowFrontend");

// POST endpoint to generate and download report
app.MapPost("/api/reports/generate", async (
    HttpContext context,
    ReportValidator validator,
    DocumentGenerator documentGenerator,
    FileNameService fileNameService) =>
{
 try
 {
  // Parse the incoming request
  var requestData = await JsonSerializer.DeserializeAsync<ReportRequest>(
      context.Request.Body,
      new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
  );

  // Validate the request data
  var (isValid, errorMessage) = validator.ValidateRequest(requestData);
  if (!isValid)
  {
   context.Response.StatusCode = 400;
   await context.Response.WriteAsJsonAsync(new { message = errorMessage });
   return;
  }

  // Generate the Word document in memory
  using var memoryStream = new MemoryStream();
  documentGenerator.CreateReportDocument(
      memoryStream,
      requestData!.ClientName!,
      requestData.ReportTypeName!,
      requestData.Year,
      requestData.ReportId ?? "N/A"
  );

  memoryStream.Position = 0;

  // Generate a safe filename
  string fileName = fileNameService.GenerateFileName(requestData.ClientName!, requestData.Year);

  // Set response headers for file download
  context.Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
  context.Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";

  // Stream the document to the client
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
