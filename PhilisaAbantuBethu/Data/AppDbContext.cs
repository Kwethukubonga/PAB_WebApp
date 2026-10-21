using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Data.Protection;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Data
{

    /// <summary>
    /// Represents the application's database context, providing access to the SupportRequests
    /// and ContactInquiry tables.
    /// </summary>
    public class AppDbContext : DbContext
    {

        // Lengths for the columns the form validators don't set a limit on themselves.
        internal const int PhoneMax = 30;
        internal const int ChoiceMax = 60;
        internal const int ReferenceMax = 20;
        
        private readonly ISensitiveText? _sensitive;

        /// <summary>
        /// Initializes a new instance of the AppDbContext class with the specified options.
        /// Used by the EF design-time tools, which build the model without the app's services.
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// The context the app itself uses. <paramref name="sensitive"/> encrypts the two columns
        /// that can describe abuse on the way in and decrypts them on the way out (A5).
        /// </summary>
        public AppDbContext(DbContextOptions<AppDbContext> options, ISensitiveText sensitive)
            : base(options) => _sensitive = sensitive;

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

                // The same limits the form checks (Forms/SupportRequestForm.cs), so anything that
                // got past the form can't be stored either.
                request.Property(r => r.FirstName).IsRequired().HasMaxLength(SupportRequestValidator.NameMax);
                request.Property(r => r.Surname).IsRequired().HasMaxLength(SupportRequestValidator.NameMax);
                request.Property(r => r.Phone).IsRequired().HasMaxLength(PhoneMax);
                request.Property(r => r.Email).IsRequired().HasMaxLength(SupportRequestValidator.EmailMax);
                request.Property(r => r.Area).IsRequired().HasMaxLength(SupportRequestValidator.AreaMax);
                request.Property(r => r.ContactMethod).IsRequired().HasMaxLength(ChoiceMax);
                request.Property(r => r.SupportType).IsRequired().HasMaxLength(ChoiceMax);
                request.Property(r => r.Urgent).IsRequired().HasMaxLength(ChoiceMax);
                request.Property(r => r.Reference).IsRequired().HasMaxLength(ReferenceMax);
                request.Property(r => r.Status).IsRequired().HasMaxLength(ChoiceMax);
                request.Property(r => r.HandledBy).HasMaxLength(SupportRequestValidator.EmailMax);
                request.Property(r => r.StaffNotes).HasMaxLength(SupportRequestValidator.StaffNotesMax);

                // Situation and Extra are what someone writes about what is happening to them.
                // They are encrypted in the database and readable only by the app holding the
                // data protection keys. Encryption makes the text longer, so these two columns
                // keep no length limit.
                request.Property(r => r.Situation).IsRequired();
                request.Property(r => r.Extra).IsRequired();
                if (_sensitive is not null)
                {
                    request.Property(r => r.Situation).HasConversion(_sensitive.Converter);
                    request.Property(r => r.Extra).HasConversion(_sensitive.Converter);
                }
            });

            modelBuilder.Entity<ContactInquiry>(inquiry =>
            {
                inquiry.HasIndex(c => c.SubmittedAt);
                inquiry.Property(c => c.Name).IsRequired().HasMaxLength(ContactValidator.NameMax);
                inquiry.Property(c => c.Email).IsRequired().HasMaxLength(SupportRequestValidator.EmailMax);
                inquiry.Property(c => c.Phone).IsRequired().HasMaxLength(PhoneMax);
                inquiry.Property(c => c.Subject).IsRequired().HasMaxLength(ChoiceMax);
                inquiry.Property(c => c.Message).IsRequired().HasMaxLength(ContactValidator.MessageMax);
            });
        }

    }
}
