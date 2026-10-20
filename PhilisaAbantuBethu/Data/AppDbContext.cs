using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Data
{

    /// <summary>
    /// Represents the application's database context, providing access to the SupportRequests 
    /// and ContactInquiry tables.
    /// </summary>
    public class AppDbContext : DbContext
    {

        /// <summary>
        /// Initializes a new instance of the AppDbContext class with the specified options.
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// Gets or sets the DbSet for SupportRequest entities, allowing CRUD operations 
        /// on the SupportRequests table in the database.
        /// </summary>
        public DbSet<SupportRequest> SupportRequests { get; set; } = null!;

        /// <summary>
        /// Gets or sets the DbSet for ContactInquiry entities, allowing CRUD operations
        /// </summary>
        public DbSet<ContactInquiry> ContactInquiries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupportRequest>(request =>
            {
                // Two people must never be given the same reference number.
                request.HasIndex(r => r.Reference).IsUnique();
                request.HasIndex(r => r.SubmittedAt);
                request.HasIndex(r => r.Status);
                request.Property(r => r.Status).HasDefaultValue(SupportRequestStatus.New);
                request.Ignore(r => r.IsUrgent);
            });

            modelBuilder.Entity<ContactInquiry>()
                .HasIndex(c => c.SubmittedAt);
        }

    }
}
