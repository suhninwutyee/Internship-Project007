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
    public DbSet<City> Cities { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<SuccessStory> SuccessStories { get; set; }
    public DbSet<ProjectType> ProjectTypes { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Framework> Frameworks { get; set; }
    public DbSet<ProjectFile> ProjectFiles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<NRCTownship> NRCTownships { get; set; }
    public DbSet<NRCType> NRCTypes { get; set; }
    public DbSet<AdminActivityLog> AdminActivityLogs { get; set; }
    public DbSet<Announcement> Announcements { get; set; }

    public DbSet<Notification> Notifications { get; set; }




    public DbSet<Email> Emails { get; set; }

    public DbSet<OTP> OTPs { get; set; }

    public DbSet<AcademicYear> AcademicYears { get; set; }


    public DbSet<AdminActivityLog> AdminActivityLog { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Notification_pkId);
            entity.Property(n => n.Notification_pkId)
                .ValueGeneratedOnAdd();

            entity.HasOne(n => n.Student)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        

        var years = new List<AcademicYear>();
        for (int year = DateTime.Now.Year; year >= 2000; year--)
        {
            years.Add(new AcademicYear
            {
                AcademicYear_pkId = year, // e.g., 2024 for "2023-2024"
                YearRange = $"{year - 1}-{year}"
            });
        }

        modelBuilder.Entity<Project>()
            .HasOne(p => p.SubmittedByStudent)
            .WithOne(s => s.SubmittedProject)
            .HasForeignKey<Project>(p => p.SubmittedByStudent_pkId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AcademicYear>().HasData(years);

        modelBuilder.Entity<Student>()
        .HasOne(e => e.Email)
        .WithMany(e => e.Students)
        .HasForeignKey(e => e.Email_PkId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Email>()
       .HasKey(e => e.Email_PkId);
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<AcademicYear>()
        .Property(a => a.IsActive)
        .HasDefaultValue(true);


        // Configure composite key for ProjectMember
        modelBuilder.Entity<ProjectMember>()
            .HasKey(pm => new { pm.Project_pkId, pm.Student_pkId });
        modelBuilder.Entity<ProjectMember>()
              .HasKey(pm => pm.ProjectMember_pkId); // ✅ Not (pm => new { pm.Student_pkId, pm.Project_pkId })
                                                    // Configure relationships

        modelBuilder.Entity<Student>()
            .HasOne(s => s.NRCType)
            .WithMany()
            .HasForeignKey(s => s.NRCType_pkId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectFile>()
            .HasOne(f => f.Project)
            .WithMany(p => p.Files)
            .HasForeignKey(f => f.Project_pkId);

        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.Student_pkId);

        modelBuilder.Entity<Framework>()
            .HasOne(f => f.Language)
            .WithMany(l => l.Frameworks)
            .HasForeignKey(f => f.Language_pkId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Language>()
     .HasOne(l => l.ProjectType)
     .WithMany(pt => pt.Languages)
     .HasForeignKey(l => l.ProjectType_pkId)
     .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<ProjectMember>()
           .HasOne(pm => pm.Student)
           .WithMany(s => s.ProjectMembers)
           .HasForeignKey(pm => pm.Student_pkId)
           .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProjectMember>().HasKey(pm => new { pm.Project_pkId, pm.Student_pkId });

        modelBuilder.Entity<ProjectMember>()
          .HasOne(pm => pm.Project)
          .WithMany(p => p.ProjectMembers)
          .HasForeignKey(pm => pm.Project_pkId)
          .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);

    }
}