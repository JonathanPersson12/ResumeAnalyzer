using System.ComponentModel.DataAnnotations;

namespace ResumeAnalyzer.Models
{
    public class FileUploadModel
    {
        [Required(ErrorMessage = "File is required")]
        [DataType(DataType.Upload)]
        public IFormFile Resume { get; set; } = null!;
        [Required(ErrorMessage = "File type is required")]
        [RegularExpression(@"^(pdf|docx|txt)$", ErrorMessage = "Invalid file type. Only PDF, DOCX, and TXT files are allowed.")]

        public IFormFile? CoverLetter { get; set; }
        [Required(ErrorMessage = "File type is required")]
        [RegularExpression(@"^(pdf|docx|txt)$", ErrorMessage = "Invalid file type. Only PDF, DOCX, and TXT files are allowed.")]
        public string JobDescription { get; set; } = string.Empty;
    }
}
