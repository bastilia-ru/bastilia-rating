using Bastilia.Rating.Database.Entities;
using AchievementTemplate = Bastilia.Rating.Database.Entities.AchievementTemplate;
using BastiliaProject = Bastilia.Rating.Database.Entities.BastiliaProject;

namespace Bastilia.Rating.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BastiliaProject> BastiliaProjects { get; set; }
    public DbSet<ProjectAdmin> ProjectAdmins { get; set; }
    public DbSet<AchievementTemplate> AchievementTemplates { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UsersBastiliaStatus> UsersBastiliaStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure composite key for ProjectAdmin
        modelBuilder.Entity<ProjectAdmin>()
            .HasKey(pa => new { pa.ProjectId, pa.UserId });

        // Configure composite key for UsersBastiliaStatus
        modelBuilder.Entity<UsersBastiliaStatus>()
            .HasKey(ubs => new { ubs.JoinrpgUserId, ubs.BeginDate });

        // Configure relationships for Achievement
        modelBuilder.Entity<Achievement>()
            .HasOne(a => a.GrantedByUser)
            .WithMany()
            .HasForeignKey(a => a.GrantedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Achievement>()
            .HasOne(a => a.RemovedByUser)
            .WithMany()
            .HasForeignKey(a => a.RemovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure enum conversions
        modelBuilder.Entity<BastiliaProject>()
            .Property(p => p.ProjectType)
            .HasConversion<string>();

        modelBuilder.Entity<BastiliaProject>()
            .Property(p => p.BrandType)
            .HasConversion<string>();

        modelBuilder.Entity<BastiliaProject>()
            .Property(p => p.ProjectIconUri)
            .HasDefaultValue("https://static.rating.bastilia.ru/bastilia-logo.jpg");

        modelBuilder.Entity<UsersBastiliaStatus>()
            .Property(ubs => ubs.StatusType)
            .HasConversion<string>();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<ProjectType>()
            .HaveConversion<string>();

        configurationBuilder.Properties<BrandType>()
            .HaveConversion<string>();

        configurationBuilder.Properties<BastiliaStatusType>()
            .HaveConversion<string>();
    }
}
