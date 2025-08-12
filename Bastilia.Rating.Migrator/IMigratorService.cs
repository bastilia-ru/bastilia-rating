namespace Bastilia.Rating.Migrator;

internal interface IMigratorService
{
    internal abstract Task MigrateAsync(CancellationToken ct);
}
