using System;
using Microsoft.EntityFrameworkCore;
using SurveyManagement.Core.Interfaces;
using SurveyManagement.Core.Models;

namespace SurveyManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<QuestionResponse> QuestionResponses { get; set; }
        public DbSet<SurveyAssignment> SurveyAssignments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Survey configuration
            modelBuilder.Entity<Survey>()
                .HasMany(s => s.Questions)
                .WithOne(q => q.Survey)
                .HasForeignKey(q => q.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Survey>()
                .HasMany(s => s.Responses)
                .WithOne(r => r.Survey)
                .HasForeignKey(r => r.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question configuration
            modelBuilder.Entity<Question>()
                .Property(q => q.Options)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            // SurveyResponse configuration
            modelBuilder.Entity<SurveyResponse>()
                .HasMany(sr => sr.Responses)
                .WithOne(qr => qr.SurveyResponse)
                .HasForeignKey(qr => qr.SurveyResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            // User configuration
            modelBuilder.Entity<User>()
                .Property(u => u.Permissions)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>()
                .HasIndex(l => l.UserId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(l => l.EntityType);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(l => l.EntityId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(l => l.Timestamp);

            // Survey indexes
            modelBuilder.Entity<Survey>()
                .HasIndex(s => s.Department);
            modelBuilder.Entity<Survey>()
                .HasIndex(s => s.Status);
            model.Entity<Survey>()
                .HasIndex(s => s.CreatedAt);
            model.Entity<Survey>()
                .HasIndex(s => s.ScheduledDeliveryTime);

            // Question indexes
            model.Entity<Question>()
                .HasIndex(q => q.SurveyId);
            model.Entity<Question>()
                .HasIndex(q => q.Type);
            model.Entity<Question>()
                .HasIndex(q => q.Order);

            // SurveyResponse indexes
            model.Entity<SurveyResponse>()
                .HasIndex(sr => sr.SurveyId);
            model.Entity<SurveyResponse>()
                .HasIndex(sr => sr.PatientId);
            model.Entity<SurveyResponse>()
                .HasIndex(sr => sr.Status);
            model.Entity<SurveyResponse>()
                .HasIndex(sr => sr.SubmittedAt);
            model.Entity<SurveyResponse>()
                .HasIndex(sr => new { sr.SurveyId, sr.PatientId });

            // QuestionResponse indexes
            model.Entity<QuestionResponse>()
                .HasIndex(qr => qr.SurveyResponseId);
            model.Entity<QuestionResponse>()
                .HasIndex(qr => qr.QuestionId);
            model.Entity<QuestionResponse>()
                .HasIndex(qr => qr.AnsweredAt);

            // SurveyAssignment indexes
            model.Entity<SurveyAssignment>()
                .HasIndex(sa => sa.SurveyId);
            model.Entity<SurveyAssignment>()
                .HasIndex(sa => sa.PatientId);
            model.Entity<SurveyAssignment>()
                .HasIndex(sa => sa.AssignedAt);
            model.Entity<SurveyAssignment>()
                .HasIndex(sa => sa.CompletedAt);
            model.Entity<SurveyAssignment>()
                .HasIndex(sa => new { sa.SurveyId, sa.PatientId });

            // Configure JSON columns
            model.Entity<Survey>()
                .Property(s => s.NotificationSettings)
                .HasColumnType("jsonb");

            model.Entity<Question>()
                .Property(q => q.Options)
                .HasColumnType("jsonb");

            model.Entity<SurveyResponse>()
                .Property(sr => sr.Metadata)
                .HasColumnType("jsonb");

            model.Entity<QuestionResponse>()
                .Property(qr => qr.Metadata)
                .HasColumnType("jsonb");
        }
    }
} 