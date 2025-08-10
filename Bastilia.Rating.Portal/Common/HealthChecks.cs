using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace JoinRpg.Portal.Infrastructure.HealthChecks;

internal static class HealthChecks
{
    internal static void MapBrHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        _ = endpoints.MapHealthChecks("/health")
            .WithMetadata(new AllowAnonymousAttribute());
        _ = endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
        {
            Predicate = (check) => check.Tags.Contains("ready"), //TODO m.b. add some probes
        }).WithMetadata(new AllowAnonymousAttribute());

        _ = endpoints.MapHealthChecks("/health/live", new HealthCheckOptions()
        {
            Predicate = (_) => false
        }).WithMetadata(new AllowAnonymousAttribute());
    }
}
