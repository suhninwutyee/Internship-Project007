namespace ProjectManagementSystem.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models;

using System;
using System.ComponentModel.DataAnnotations;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<StudentDepartment> StudentDepartments { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<ProjectType> ProjectTypes { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Framework> Frameworks { get; set; }
    public DbSet<ProjectFile> ProjectFiles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    public DbSet<NRCTownship> NRCTownships { get; set; }
    public DbSet<NRCType> NRCTypes { get; set; }

    public DbSet<InternCom> InternComs { get; set; }

    public DbSet<Email> Emails { get; set; }


    public DbSet<AdminActivityLog> AdminActivityLogs { get; set; }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

      

        modelBuilder.Entity<Email>()
       .HasKey(e => e.Email_PkId);
        base.OnModelCreating(modelBuilder);

        

        // Configure composite key for ProjectMember
        modelBuilder.Entity<ProjectMember>()
            .HasKey(pm => new { pm.Project_pkId, pm.Student_pkId });

        // Configure relationships
      

        modelBuilder.Entity<Student>()
            .HasOne(s => s.NRCType)
            .WithMany()
            .HasForeignKey(s => s.NRCType_pkId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectFile>()
    .HasOne(f => f.Project)
    .WithMany(p => p.Files)  // Match the collection property name
    .HasForeignKey(f => f.Project_pkId);

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.Project_pkId);

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.Student)
            .WithMany()
            .HasForeignKey(pm => pm.Student_pkId);

        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.Student_pkId);

        modelBuilder.Entity<Framework>()
            .HasOne(f => f.Language)
            .WithMany(l => l.Frameworks)
            .HasForeignKey(f => f.Language_pkId)
            .OnDelete(DeleteBehavior.Restrict);

        

        modelBuilder.Entity<ProjectMember>().HasKey(pm => new { pm.Project_pkId, pm.Student_pkId });

        modelBuilder.Entity<InternCom>().ToTable("InternCom");

        base.OnModelCreating(modelBuilder);


    }
}
