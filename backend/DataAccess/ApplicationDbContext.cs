using Microsoft.EntityFrameworkCore;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Pgvector.EntityFrameworkCore;

namespace DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Personal> Personal { get; set; }
        public DbSet<Sportsman> Sportsmen { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<TrainingMetrics> TrainingMetrics { get; set; }
        public DbSet<PlanTraining> PlanTrainings { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchEvent> MatchEvents { get; set; }
        public DbSet<Normative> Normatives { get; set; }
        public DbSet<NormativeSportsman> NormativeSportsmen { get; set; }
        public DbSet<LocalNormative> LocalNormatives { get; set; }
        public DbSet<LocalNormativeSportsman> LocalNormativeSportsmen { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Broadcast> Broadcasts { get; set; }
        public DbSet<PersonalWorkout> PersonalWorkouts { get; set; }
        public DbSet<SportsmanGroup> SportsmanGroups { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<PlayerEmbedding> PlayerEmbeddings { get; set; }
        public DbSet<AiChat> AiChats { get; set; }
        public DbSet<AiMessage> AiMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // pgvector extension
            modelBuilder.HasPostgresExtension("vector");

            // PlayerEmbedding
            modelBuilder.Entity<PlayerEmbedding>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Embedding).HasColumnType("vector(1536)");
                e.HasIndex(x => x.Embedding).HasMethod("ivfflat")
                    .HasOperators("vector_cosine_ops")
                    .HasStorageParameter("lists", 10);
                e.HasOne(x => x.Sportsman)
                    .WithMany()
                    .HasForeignKey(x => x.SportsmanId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // AiChat / AiMessage
            modelBuilder.Entity<AiChat>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Trainer)
                    .WithMany()
                    .HasForeignKey(x => x.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Messages)
                    .WithOne(x => x.Chat)
                    .HasForeignKey(x => x.ChatId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AiMessage>(e =>
            {
                e.HasKey(x => x.Id);
            });

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var createdAtProperty = entityType.FindProperty("CreatedAt");
                if (createdAtProperty != null && createdAtProperty.ClrType == typeof(DateTime))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property("CreatedAt")
                        .HasDefaultValueSql("NOW()");
                }
            }

            // User
            modelBuilder.Entity<User>()
                .HasOne(u => u.Sportsman)
                .WithOne(s => s.User)
                .HasForeignKey<Sportsman>(s => s.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Personal)
                .WithOne(p => p.User)
                .HasForeignKey<Personal>(p => p.UserId);

            // Group
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Trainer)
                .WithMany(p => p.Groups)
                .HasForeignKey(g => g.TrainerId);

            modelBuilder.Entity<SportsmanGroup>()
                .HasKey(sg => new { sg.SportsmanId, sg.GroupId });

            modelBuilder.Entity<SportsmanGroup>()
                .HasOne(sg => sg.Sportsman)
                .WithMany(s => s.SportsmanGroups)
                .HasForeignKey(sg => sg.SportsmanId);

            modelBuilder.Entity<SportsmanGroup>()
                .HasOne(sg => sg.Group)
                .WithMany(g => g.SportsmanGroups)
                .HasForeignKey(sg => sg.GroupId);

            // Team
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Trainer)
                .WithMany(p => p.Teams)
                .HasForeignKey(t => t.TrainerId);

            modelBuilder.Entity<Team>()
                .HasMany(t => t.Sportsmen)
                .WithOne(s => s.Team)
                .HasForeignKey(s => s.TeamId);

            // Training
            modelBuilder.Entity<Training>()
                .HasOne(t => t.Trainer)
                .WithMany(p => p.Trainings)
                .HasForeignKey(t => t.TrainerId);

            modelBuilder.Entity<Training>()
                .HasOne(t => t.PlanTraining)
                .WithMany(pt => pt.Trainings)
                .HasForeignKey(t => t.PlanTrainingId);

            modelBuilder.Entity<Training>()
                .HasOne(t => t.Group)
                .WithMany()
                .HasForeignKey(t => t.GroupId);

            // TrainingMetrics
            modelBuilder.Entity<TrainingMetrics>()
                .HasOne(tm => tm.Training)
                .WithMany(t => t.Metrics)
                .HasForeignKey(tm => tm.TrainingId);

            modelBuilder.Entity<TrainingMetrics>()
                .HasOne(tm => tm.Sportsman)
                .WithMany(s => s.TrainingMetrics)
                .HasForeignKey(tm => tm.SportsmanId);

            // PlanTraining
            modelBuilder.Entity<PlanTraining>()
                .HasOne(pt => pt.Trainer)
                .WithMany(p => p.PlanTrainings)
                .HasForeignKey(pt => pt.TrainerId);

            // Match
            modelBuilder.Entity<Match>()
                .Property(m => m.HomeTeamId)
                .IsRequired();

            modelBuilder.Entity<Match>()
                .Property(m => m.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Match>()
                .Property(m => m.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Match>()
                .Property(m => m.Result)
                .HasConversion<string>();

            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomeTeam)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.HomeTeamId);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.OpponentTeam)
                .WithMany()
                .HasForeignKey(m => m.OpponentTeamId);

            // Training ↔ Match: один матч может иметь N тренировок (по одной на группу).
            // FK живёт в Training.MatchId. При удалении Match — Training.MatchId = NULL
            // (саму Training удаляет сервис каскадно если Type=="Матч").
            modelBuilder.Entity<Training>()
                .HasOne(t => t.Match)
                .WithMany(m => m.Trainings)
                .HasForeignKey(t => t.MatchId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Match>()
                .OwnsMany(m => m.Lineup, l =>
                {
                    l.ToJson();
                });

            modelBuilder.Entity<Match>()
                .OwnsMany(m => m.Lineup, l => { l.ToJson(); });

            // MatchStatistics удалена — статистика считается из MatchEvent

            // MatchEvent
            modelBuilder.Entity<MatchEvent>()
                .HasOne(me => me.Match)
                .WithMany(m => m.Events)
                .HasForeignKey(me => me.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchEvent>()
                .Property(me => me.Type)
                .HasConversion<string>();

            // Class
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Group)
                .WithMany()
                .HasForeignKey(c => c.GroupId);

            // Normative
            modelBuilder.Entity<NormativeSportsman>()
                .HasOne(ns => ns.Sportsman)
                .WithMany(s => s.Normatives)
                .HasForeignKey(ns => ns.SportsmanId);

            modelBuilder.Entity<NormativeSportsman>()
                .HasOne(ns => ns.Normative)
                .WithMany(n => n.NormativeSportsmen)
                .HasForeignKey(ns => ns.NormativeId)
                .OnDelete(DeleteBehavior.Cascade);

            // LocalNormative
            modelBuilder.Entity<LocalNormativeSportsman>()
                .HasOne(ns => ns.Sportsman)
                .WithMany(s => s.LocalNormatives)
                .HasForeignKey(ns => ns.SportsmanId);

            modelBuilder.Entity<LocalNormativeSportsman>()
                .HasOne(ns => ns.LocalNormative)
                .WithMany(n => n.LocalNormativeSportsmen)
                .HasForeignKey(ns => ns.LocalNormativeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sportsman>()
                .Property(s => s.Position)
                .HasConversion<string>();

            // Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Broadcast)
                .WithMany(b => b.Messages)
                .HasForeignKey(m => m.BroadcastId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Message>()
                .Property(m => m.SenderRole)
                .HasConversion<string>();

            // Broadcast
            modelBuilder.Entity<Broadcast>()
                .HasOne(b => b.CreatedBy)
                .WithMany()
                .HasForeignKey(b => b.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Broadcast>()
                .Property(b => b.TargetType)
                .HasConversion<string>();

            modelBuilder.Entity<Broadcast>()
                .Property(b => b.CreatedByRole)
                .HasConversion<string>();

            // PersonalWorkout
            modelBuilder.Entity<PersonalWorkout>()
                .HasOne(pw => pw.Sportsman)
                .WithMany(s => s.PersonalWorkouts)
                .HasForeignKey(pw => pw.SportsmanId);

            modelBuilder.Entity<PersonalWorkout>()
                .HasOne(pw => pw.Personal)
                .WithMany(p => p.PersonalWorkouts)
                .HasForeignKey(pw => pw.PersonalId);

            // Attendance
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Training)
                .WithMany()
                .HasForeignKey(a => a.TrainingId);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Sportsman)
                .WithMany()
                .HasForeignKey(a => a.SportsmanId);

            modelBuilder.Entity<Attendance>()
                .Property(a => a.Status)
                .HasConversion<string>();

            // Image
            modelBuilder.Entity<Image>()
                .HasOne(i => i.Sportsman)
                .WithMany(s => s.Images)
                .HasForeignKey(i => i.SportsmanId);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.Personal)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.PersonalId);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.Team)
                .WithMany(t => t.Images)
                .HasForeignKey(i => i.TeamId);
        }
    }
}
