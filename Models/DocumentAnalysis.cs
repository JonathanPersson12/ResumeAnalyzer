namespace ResumeAnalyzer.Models
{
    public class DocumentAnalysis
    {
        public double ResumeMatchScore { get; set; }
        public double CoverLetterMatchScore { get; set; }
        public List<string> FoundKeywords { get; set; } = new();
        public List<string> MissingKeywords { get; set; } = new();
        public List<string> RecommendedKeywords { get; set; } = new();
        public List<string> ActionableFeedback { get; set; } = new();
        public List<string> ConsistencyIssues { get; set; } = new();
        public ATSAnalysis ATSResults { get; set; } = new();
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class ATSAnalysis
    {
        public double ATSScore { get; set; }
        public List<string> ATSIssues { get; set; } = new();
        public List<string> ATSRecommendations { get; set; } = new();
        public bool HasContactInfo { get; set; }
        public bool HasProperFormatting { get; set; }
    }
}