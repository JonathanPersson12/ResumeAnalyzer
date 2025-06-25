using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Services;
using System.Security.Claims;
using System.Text.Json;

namespace ResumeAnalyzer.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly TextExtractionService _textExtractionService;
        private readonly DocumentAnalysisService _analysisService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FileController(
            TextExtractionService textExtractionService,
            DocumentAnalysisService analysisService,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _textExtractionService = textExtractionService;
            _analysisService = analysisService;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("analyze")]
        [Authorize]
        public async Task<IActionResult> AnalyzeDocuments([FromForm] FileUploadModel model)
        {
            try
            {
                if (model.Resume == null || model.Resume.Length == 0)
                    return BadRequest("No resume uploaded.");

                if (string.IsNullOrEmpty(model.JobDescription))
                    return BadRequest("Job description is required.");

                // Extract text from files
                var resumeText = await _textExtractionService.ExtractTextFromFile(model.Resume);
                var coverLetterText = string.Empty;

                if (model.CoverLetter != null && model.CoverLetter.Length > 0)
                {
                    coverLetterText = await _textExtractionService.ExtractTextFromFile(model.CoverLetter);
                }

                // Perform analysis
                var analysis = await _analysisService.AnalyzeDocuments(
                    resumeText,
                    coverLetterText,
                    model.JobDescription);

                // Save to database
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var analysisHistory = new AnalysisHistory
                {
                    UserId = userId,
                    ResumeMatchScore = analysis.ResumeMatchScore,
                    CoverLetterMatchScore = analysis.CoverLetterMatchScore,
                    AnalysisResultJson = JsonSerializer.Serialize(analysis),
                    ResumeFileName = model.Resume.FileName,
                    CoverLetterFileName = model.CoverLetter?.FileName ?? ""
                };

                _context.AnalysisHistories.Add(analysisHistory);
                await _context.SaveChangesAsync();

                return Ok(analysis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during analysis", error = ex.Message });
            }
        }

        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetAnalysisHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var history = _context.AnalysisHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new
                {
                    h.Id,
                    h.CreatedAt,
                    h.JobTitle,
                    h.CompanyName,
                    h.ResumeMatchScore,
                    h.CoverLetterMatchScore,
                    h.ResumeFileName,
                    h.CoverLetterFileName
                })
                .ToList();

            return Ok(history);
        }

        [HttpGet("history/{id}")]
        [Authorize]
        public async Task<IActionResult> GetAnalysisDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var analysis = _context.AnalysisHistories
                .Where(h => h.UserId == userId && h.Id == id)
                .FirstOrDefault();

            if (analysis == null)
                return NotFound();

            var result = JsonSerializer.Deserialize<DocumentAnalysis>(analysis.AnalysisResultJson);
            return Ok(result);
        }
    }
}