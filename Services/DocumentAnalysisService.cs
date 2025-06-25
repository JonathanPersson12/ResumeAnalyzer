using ResumeAnalyzer.Models;
using System.Text.RegularExpressions;

namespace ResumeAnalyzer.Services
{
    public class DocumentAnalysisService
    {
        private readonly HashSet<string> _stopWords = new()
        {
            "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "is", "are", "was", "were", "be", "been", "have", "has", "had", "do", "does", "did", "will", "would", "could", "should", "may", "might", "must", "can", "this", "that", "these", "those"
        };

        public async Task<DocumentAnalysis> AnalyzeDocuments(
            string resumeText,
            string coverLetterText,
            string jobDescription,
            string jobTitle = "",
            string companyName = "")
        {
            var analysis = new DocumentAnalysis();

            // Extract keywords from job description
            var jobKeywords = ExtractKeywords(jobDescription);
            var resumeKeywords = ExtractKeywords(resumeText);
            var coverLetterKeywords = ExtractKeywords(coverLetterText);

            // Calculate match scores
            analysis.ResumeMatchScore = CalculateMatchScore(resumeKeywords, jobKeywords);
            analysis.CoverLetterMatchScore = string.IsNullOrEmpty(coverLetterText) ? 0 :
                CalculateMatchScore(coverLetterKeywords, jobKeywords);

            // Find keywords
            analysis.FoundKeywords = resumeKeywords.Intersect(jobKeywords).ToList();
            analysis.MissingKeywords = jobKeywords.Except(resumeKeywords).Take(10).ToList();

            // Generate feedback
            analysis.ActionableFeedback = GenerateActionableFeedback(analysis, resumeText, coverLetterText, jobDescription);

            // Check consistency
            analysis.ConsistencyIssues = CheckConsistency(resumeText, coverLetterText);

            // ATS Analysis
            analysis.ATSResults = AnalyzeATS(resumeText);

            return analysis;
        }

        private List<string> ExtractKeywords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            // Clean and split text
            var words = Regex.Replace(text.ToLower(), @"[^\w\s]", " ")
                .Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 2 && !_stopWords.Contains(word))
                .GroupBy(word => word)
                .Where(group => group.Count() >= 1) // Keep words that appear at least once
                .Select(group => group.Key)
                .ToList();

            return words;
        }

        private double CalculateMatchScore(List<string> documentKeywords, List<string> jobKeywords)
        {
            if (!jobKeywords.Any())
                return 0;

            var matches = documentKeywords.Intersect(jobKeywords).Count();
            return Math.Round((double)matches / jobKeywords.Count * 100, 2);
        }

        private List<string> GenerateActionableFeedback(DocumentAnalysis analysis, string resumeText, string coverLetterText, string jobDescription)
        {
            var feedback = new List<string>();

            // Score-based feedback
            if (analysis.ResumeMatchScore < 40)
            {
                feedback.Add("🔴 Your resume has a low match score. Consider adding more relevant keywords from the job description.");
            }
            else if (analysis.ResumeMatchScore < 70)
            {
                feedback.Add("🟡 Your resume has a moderate match score. Adding a few more relevant skills could improve your chances.");
            }
            else
            {
                feedback.Add("🟢 Great! Your resume has a strong match with the job requirements.");
            }

            // Missing keywords feedback
            if (analysis.MissingKeywords.Any())
            {
                var topMissing = analysis.MissingKeywords.Take(5);
                feedback.Add($"💡 Consider adding these important keywords: {string.Join(", ", topMissing)}");
            }

            // Cover letter feedback
            if (string.IsNullOrEmpty(coverLetterText))
            {
                feedback.Add("📝 Consider adding a cover letter to strengthen your application.");
            }
            else if (analysis.CoverLetterMatchScore < 30)
            {
                feedback.Add("📝 Your cover letter could better reflect the job requirements. Try incorporating more relevant keywords.");
            }

            // Length feedback
            if (resumeText.Length < 500)
            {
                feedback.Add("📄 Your resume seems quite short. Consider adding more details about your experience and skills.");
            }

            // ATS feedback
            if (analysis.ATSResults.ATSScore < 60)
            {
                feedback.Add("🤖 Your resume may have issues with Applicant Tracking Systems (ATS). Check the ATS analysis for details.");
            }

            return feedback;
        }

        private List<string> CheckConsistency(string resumeText, string coverLetterText)
        {
            var issues = new List<string>();

            if (string.IsNullOrEmpty(coverLetterText))
                return issues;

            var resumeKeywords = ExtractKeywords(resumeText);
            var coverLetterKeywords = ExtractKeywords(coverLetterText);

            // Check if cover letter mentions skills not in resume
            var coverLetterOnlySkills = coverLetterKeywords.Except(resumeKeywords).ToList();
            if (coverLetterOnlySkills.Count > 5)
            {
                issues.Add("Your cover letter mentions skills not found in your resume. Ensure consistency between documents.");
            }

            // Check if resume has many skills not mentioned in cover letter
            var resumeOnlySkills = resumeKeywords.Except(coverLetterKeywords).ToList();
            if (resumeOnlySkills.Count > 10)
            {
                issues.Add("Consider mentioning more of your resume skills in your cover letter to create a cohesive narrative.");
            }

            return issues;
        }

        private ATSAnalysis AnalyzeATS(string resumeText)
        {
            var ats = new ATSAnalysis();
            var issues = new List<string>();
            var recommendations = new List<string>();

            // Check for contact information
            var hasEmail = Regex.IsMatch(resumeText, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");
            var hasPhone = Regex.IsMatch(resumeText, @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b");

            ats.HasContactInfo = hasEmail && hasPhone;

            if (!hasEmail)
            {
                issues.Add("No email address found");
                recommendations.Add("Add your email address to your resume");
            }

            if (!hasPhone)
            {
                issues.Add("No phone number found");
                recommendations.Add("Add your phone number to your resume");
            }

            // Check for common ATS-friendly sections
            var hasSummary = resumeText.ToLower().Contains("summary") || resumeText.ToLower().Contains("objective");
            var hasExperience = resumeText.ToLower().Contains("experience") || resumeText.ToLower().Contains("work");
            var hasEducation = resumeText.ToLower().Contains("education") || resumeText.ToLower().Contains("degree");
            var hasSkills = resumeText.ToLower().Contains("skills") || resumeText.ToLower().Contains("technical");

            if (!hasSummary)
            {
                recommendations.Add("Consider adding a professional summary section");
            }

            if (!hasExperience)
            {
                issues.Add("No clear work experience section found");
            }

            if (!hasEducation)
            {
                recommendations.Add("Consider adding an education section");
            }

            if (!hasSkills)
            {
                recommendations.Add("Consider adding a skills section");
            }

            // Calculate ATS score
            var score = 0;
            if (ats.HasContactInfo) score += 30;
            if (hasSummary) score += 15;
            if (hasExperience) score += 25;
            if (hasEducation) score += 15;
            if (hasSkills) score += 15;

            ats.ATSScore = score;
            ats.ATSIssues = issues;
            ats.ATSRecommendations = recommendations;
            ats.HasProperFormatting = score >= 70;

            return ats;
        }
    }
}