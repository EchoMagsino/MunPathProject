using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MunPath.Models.EmailVerification;
using MunPath.Models.Roles;
using MunPath.Models.User;


namespace MunPath.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) {}

        public DbSet<RoleModel> Roles {get; set;}
        public DbSet<UserModel> Users {get; set;}
        public DbSet<EmailVerificationModel> EmailVerifications {get; set;}
  



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<UserModel>()
        .HasOne(user => user.Role)
        .WithMany()
        .HasForeignKey(user => user.RoleName)
        .IsRequired();

      modelBuilder.Entity<UserModel>()
        .HasIndex(user => user.Email)
        .IsUnique();

    }
    
    }
}