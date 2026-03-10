using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Silo;

public class SiloHealthcheck : IHealthCheck
{
    private readonly ISiloStatusOracle _siloStatusOracle;

    public SiloHealthcheck(ISiloStatusOracle siloStatusOracle)
    {
        _siloStatusOracle = siloStatusOracle;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var status = _siloStatusOracle.CurrentStatus;

        return Task.FromResult(status switch
        {
            SiloStatus.Active => HealthCheckResult.Healthy("Silo actif"),
            SiloStatus.ShuttingDown or
            SiloStatus.Stopping => HealthCheckResult.Degraded($"Silo en cours d'arrêt : {status}"),
            _ => HealthCheckResult.Unhealthy($"Silo non disponible : {status}")
        });
    }
}
