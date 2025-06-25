using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using Path = System.IO.Path;

namespace ResumeAnalyzer.Services
{
    public class TextExtractionService
    {
        public async Task<string> ExtractTextFromFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var extension = Path.GetExtension(file.FileName).ToLower();

            using var stream = file.OpenReadStream();

            return extension switch
            {
                ".pdf" => await ExtractFromPdf(stream),
                ".docx" => await ExtractFromDocx(stream),
                ".txt" => await ExtractFromTxt(stream),
                _ => throw new NotSupportedException($"File type {extension} not supported")
            };
        }

        private async Task<string> ExtractFromPdf(Stream stream)
        {
            try
            {
                using var reader = new PdfReader(stream);
                var text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                    text.Append(" ");
                }

                return text.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting text from PDF: {ex.Message}");
            }
        }

        private async Task<string> ExtractFromDocx(Stream stream)
        {
            try
            {
                using var document = WordprocessingDocument.Open(stream, false);
                var body = document.MainDocumentPart?.Document.Body;
                return body?.InnerText ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting text from DOCX: {ex.Message}");
            }
        }

        private async Task<string> ExtractFromTxt(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}