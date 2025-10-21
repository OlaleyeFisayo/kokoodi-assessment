using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FinancialReportGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Financial Report Generator - Backend Demo");
            Console.WriteLine("===========================================\n");

            try
            {
                Console.WriteLine("Generating sample reports...\n");

                CreateSimpleReport("Acme Corporation", "Profit & Loss");
                Console.WriteLine("✓ Report 1: P&L for Acme Corporation");

                CreateSimpleReport("TechStart Inc", "Balance Sheet");
                Console.WriteLine("✓ Report 2: Balance Sheet for TechStart Inc");

                CreateSimpleReport("Global Enterprises", "Cash Flow Statement");
                Console.WriteLine("✓ Report 3: Cash Flow for Global Enterprises");

                Console.WriteLine("\n===========================================");
                Console.WriteLine("All reports generated successfully!");
                Console.WriteLine("===========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Creates a Word document with report content using OpenXML SDK
        /// </summary>
        static void CreateSimpleReport(string clientName, string reportType)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client name cannot be empty", nameof(clientName));

            if (string.IsNullOrWhiteSpace(reportType))
                throw new ArgumentException("Report type cannot be empty", nameof(reportType));

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string safeClientName = string.Join("_", clientName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"GeneratedReport_{safeClientName}_{timestamp}.docx";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            using (WordprocessingDocument wordDocument =
                WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Main report title
                Paragraph para = body.AppendChild(new Paragraph());
                Run run = para.AppendChild(new Run());

                RunProperties runProperties = run.AppendChild(new RunProperties());
                runProperties.AppendChild(new Bold());
                runProperties.AppendChild(new FontSize() { Val = "28" });

                string reportContent = $"Report: {reportType} for Client: {clientName}";
                run.AppendChild(new Text(reportContent));

                // Metadata paragraph
                Paragraph metaPara = body.AppendChild(new Paragraph());
                Run metaRun = metaPara.AppendChild(new Run());
                RunProperties metaRunProps = metaRun.AppendChild(new RunProperties());
                metaRunProps.AppendChild(new Italic());
                metaRunProps.AppendChild(new FontSize() { Val = "20" });

                string metadata = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                metaRun.AppendChild(new Text(metadata));

                mainPart.Document.Save();
            }

            Console.WriteLine($"   → File saved: {fileName}");
        }
    }
}
