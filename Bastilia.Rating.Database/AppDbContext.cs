using Bastilia.Rating.Database.Entities;
using JoinRpg.Common.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using AchievementTemplate = Bastilia.Rating.Database.Entities.AchievementTemplate;
using BastiliaProject = Bastilia.Rating.Database.Entities.BastiliaProject;

namespace Bastilia.Rating.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<BastiliaProject> BastiliaProjects { get; set; }
    public DbSet<ProjectAdmin> ProjectAdmins { get; set; }
    public DbSet<AchievementTemplate> AchievementTemplates { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UsersBastiliaStatus> UsersBastiliaStatuses { get; set; }
    public DbSet<ClubEvent> ClubEvents { get; set; }
    public DbSet<KogdaIgraGame> KogdaIgraGames { get; set; }
    public DbSet<ProjectNewsEntity> ProjectNews { get; set; }
    public DbSet<DailyJobRunEntity> DailyJobRuns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Model.FindEntityType(typeof(Achievement));

        // Configure composite key for ProjectAdmin
        modelBuilder.Entity<ProjectAdmin>()
            .HasKey(pa => new { pa.ProjectId, pa.UserId });

        // Configure composite key for UsersBastiliaStatus
        modelBuilder.Entity<UsersBastiliaStatus>()
            .HasKey(ubs => new { ubs.JoinrpgUserId, ubs.BeginDate });

        // Configure relationships for Achievement
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(a => a.AchievementId);

            entity.Property(a => a.AchievementId)
                .UseIdentityByDefaultColumn()  // ← ключевое
                .ValueGeneratedOnAdd();

            entity.HasOne(a => a.GrantedByUser)
                .WithMany()
                .HasForeignKey(a => a.GrantedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.RemovedByUser)
                .WithMany()
                .HasForeignKey(a => a.RemovedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Конвертация typed-id свойств (реализующих IEntityId<TSelf,TValue>) зарегистрирована в
        // ConfigureConventions — но идентифицировать однопроцессные PK как identity-колонки нужно уже здесь,
        // когда модель построена. Покрывает и UserIdentification (User.JoinRpgUserId).
        modelBuilder.EntityIdsSetValueGeneratedOnAdd();

        // Configure enum conversions
        modelBuilder.Entity<BastiliaProject>()
            .Property(p => p.ProjectType)
            .HasConversion<string>();

        modelBuilder.Entity<BastiliaProject>()
            .Property(p => p.BrandType)
            .HasConversion<string>();

        modelBuilder.Entity<BastiliaProject>()
            .Property(p => p.ProjectLevel)
            .HasConversion<string>();

        modelBuilder.Entity<BastiliaProject>()
            .Property(p => p.NewsCadence)
            .HasDefaultValue(NewsCadence.Monthly);

        modelBuilder.Entity<BastiliaProject>()
            .Property(p => p.ProjectIconUri)
            .HasDefaultValue("https://static.rating.bastilia.ru/bastilia-logo.jpg");

        modelBuilder.Entity<UsersBastiliaStatus>()
            .Property(ubs => ubs.StatusType)
            .HasConversion<string>();

        modelBuilder.Entity<ClubEvent>(entity =>
        {
            entity.ToTable("ClubEvents");

            entity.Property(e => e.EventType)
                .HasConversion<string>();

            entity.HasOne(e => e.User)
                .WithMany(u => u.ClubEvents)
                .HasForeignKey(e => e.JoinRpgUserId)
                .IsRequired(false);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .IsRequired(false);
        });

        modelBuilder.Entity<ProjectNewsEntity>(entity =>
        {
            entity.HasKey(x => x.ProjectNewsId);

            entity.HasIndex(x => new { x.ProjectId, x.CreatedAt });

            entity.HasOne(x => x.Project)
                .WithMany(p => p.ProjectNews)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(x => x.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.SendStatus)
                .HasConversion<string>();
        });

        modelBuilder.Entity<DailyJobRunEntity>(entity =>
        {
            entity.HasKey(x => x.DailyJobRunId);
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<ProjectType>()
            .HaveConversion<string>();

        configurationBuilder.Properties<BrandType>()
            .HaveConversion<string>();

        configurationBuilder.Properties<BastiliaStatusType>()
            .HaveConversion<string>();

        // По одной строке на каждый typed-id (HaveEntityIdValueConversion<TId, TValue> — общий generic-конвертер
        // из JoinRpg.Common.EntityFrameworkCore). Регистрация обязана происходить именно тут, в ConfigureConventions
        // (до построения модели) — иначе EF ещё не знает, что это скалярный тип, и на этапе обнаружения
        // сущностей ошибочно принимает такое свойство за навигацию к новой entity. TValue не выводится
        // из TId, поэтому указывается явно.
        configurationBuilder.Properties<BastiliaProjectId>()
            .HaveEntityIdValueConversion<BastiliaProjectId, int>();

        configurationBuilder.Properties<TemplateId>()
            .HaveEntityIdValueConversion<TemplateId, int>();

        configurationBuilder.Properties<UserIdentification>()
            .HaveEntityIdValueConversion<UserIdentification, int>();
    }
}
