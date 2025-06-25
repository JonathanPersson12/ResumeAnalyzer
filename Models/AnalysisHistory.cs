using System.ComponentModel.DataAnnotations;

namespace ResumeAnalyzer.Models
{
    public class AnalysisHistory
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        public double ResumeMatchScore { get; set; }
        public double CoverLetterMatchScore { get; set; }

        // Store the full analysis as JSON
        public string AnalysisResultJson { get; set; } = string.Empty;

        public string ResumeFileName { get; set; } = string.Empty;
        public string CoverLetterFileName { get; set; } = string.Empty;
    }
}