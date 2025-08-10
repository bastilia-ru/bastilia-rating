using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace Bastilia.Rating.Database;

public abstract class JoinPostgreSqlEfContextBase(DbContextOptions options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseExceptionProcessor(); // sane index exceptions
        base.OnConfiguring(optionsBuilder);
    }
}
