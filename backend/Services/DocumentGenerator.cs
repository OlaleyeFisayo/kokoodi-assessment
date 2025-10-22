namespace Backend.Services;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

/// <summary>
/// Handles the creation of professional financial reports as Word documents
/// </summary>
public class DocumentGenerator
{
    /// <summary>
    /// Generates a formatted Word document containing the financial report
    /// </summary>
    public void CreateReportDocument(Stream stream, string clientName, string reportType, int year, string reportId)
    {
        using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            AddTitle(body, reportType);
            AddClientInfo(body, clientName);
            AddYearInfo(body, year);
            AddSpacing(body);
            AddMetadata(body, reportId);
            AddSpacing(body);
            AddFinancialSummary(body);

            mainPart.Document.Save();
        }
    }

    /// <summary>
    /// Adds the main title at the top of the document
    /// </summary>
    private void AddTitle(Body body, string reportType)
    {
        Paragraph titlePara = body.AppendChild(new Paragraph());
        Run titleRun = titlePara.AppendChild(new Run());

        RunProperties titleProps = titleRun.AppendChild(new RunProperties());
        titleProps.AppendChild(new Bold());
        titleProps.AppendChild(new FontSize() { Val = "32" });
        titleProps.AppendChild(new Color() { Val = "1a1a1a" });

        titleRun.AppendChild(new Text($"{reportType} Report"));
    }

    /// <summary>
    /// Adds client information paragraph
    /// </summary>
    private void AddClientInfo(Body body, string clientName)
    {
        Paragraph clientPara = body.AppendChild(new Paragraph());
        Run clientRun = clientPara.AppendChild(new Run());

        RunProperties clientProps = clientRun.AppendChild(new RunProperties());
        clientProps.AppendChild(new FontSize() { Val = "24" });

        clientRun.AppendChild(new Text($"Client: {clientName}"));
    }

    /// <summary>
    /// Adds reporting year paragraph
    /// </summary>
    private void AddYearInfo(Body body, int year)
    {
        Paragraph yearPara = body.AppendChild(new Paragraph());
        Run yearRun = yearPara.AppendChild(new Run());

        RunProperties yearProps = yearRun.AppendChild(new RunProperties());
        yearProps.AppendChild(new FontSize() { Val = "24" });

        yearRun.AppendChild(new Text($"Reporting Year: {year}"));
    }

    /// <summary>
    /// Adds metadata like generation timestamp and report ID
    /// </summary>
    private void AddMetadata(Body body, string reportId)
    {
        Paragraph metaPara = body.AppendChild(new Paragraph());
        Run metaRun = metaPara.AppendChild(new Run());

        RunProperties metaRunProps = metaRun.AppendChild(new RunProperties());
        metaRunProps.AppendChild(new Italic());
        metaRunProps.AppendChild(new FontSize() { Val = "20" });
        metaRunProps.AppendChild(new Color() { Val = "737373" });

        string metadata = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\nReport ID: {reportId}";
        metaRun.AppendChild(new Text(metadata) { Space = SpaceProcessingModeValues.Preserve });
    }

    /// <summary>
    /// Adds the main content section with financial summary
    /// </summary>
    private void AddFinancialSummary(Body body)
    {
        Paragraph contentHeaderPara = body.AppendChild(new Paragraph());
        Run contentHeaderRun = contentHeaderPara.AppendChild(new Run());

        RunProperties contentHeaderProps = contentHeaderRun.AppendChild(new RunProperties());
        contentHeaderProps.AppendChild(new Bold());
        contentHeaderProps.AppendChild(new FontSize() { Val = "24" });

        contentHeaderRun.AppendChild(new Text("Financial Summary"));

        Paragraph contentPara = body.AppendChild(new Paragraph());
        Run contentRun = contentPara.AppendChild(new Run());
        contentRun.AppendChild(new Text(
            "This is a sample financial report generated using the OpenXML SDK. "
        ));
    }

    /// <summary>
    /// Adds a blank line for visual spacing
    /// </summary>
    private void AddSpacing(Body body)
    {
        body.AppendChild(new Paragraph());
    }
}
