using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add this line to your existing DbContext
        public DbSet<AnalysisHistory> AnalysisHistories { get; set; }
    }
}