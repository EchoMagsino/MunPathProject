
using Microsoft.EntityFrameworkCore;
using MunPath.Models.EmailVerification;
using MunPath.Models.Roles;
using MunPath.Models.User;
using MunPathProject.Models;

namespace MunPath.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<EmailVerificationModel> EmailVerifications { get; set; }
        public DbSet<StudentAcademicRecord> studentAcademicRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Role relationship
            modelBuilder.Entity<UserModel>()
                .HasOne(user => user.Role)
                .WithMany()
                .HasForeignKey(user => user.RoleName)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Unique email constraint
            modelBuilder.Entity<UserModel>()
                .HasIndex(user => user.Email)
                .IsUnique();

            // User - StudentAcademicRecord one-to-one relationship
            modelBuilder.Entity<StudentAcademicRecord>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<StudentAcademicRecord>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentAcademicRecord>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            // Email verification constraints
            modelBuilder.Entity<EmailVerificationModel>(entity =>
            {
                entity.ToTable("email_verifications", table =>
                {
                    table.HasCheckConstraint(
                        "CK_email_verifications_attempts",
                        "attempts >= 0"
                    );
                });

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.OtpHash)
                    .HasColumnName("otp_hash")
                    .HasMaxLength(64)
                    .IsRequired();

                entity.Property(e => e.ExpiresAtUtc)
                    .HasColumnName("expires_at_utc")
                    .IsRequired();

                entity.Property(e => e.Attempts)
                    .HasColumnName("attempts")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.CreatedAtUtc)
                    .HasColumnName("created_at_utc")
                    .IsRequired();

                entity.Property(e => e.IsUsed)
                    .HasColumnName("is_used")
                    .HasDefaultValue(false)
                    .IsRequired();

                // Index for looking up verifications by user
                entity.HasIndex(e => e.UserId);

                // User - EmailVerification relationship
                entity.HasOne(e => e.users)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
