using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Silo;

public class SiloHealthcheck : IHealthCheck
{
    private readonly IClusterClient _clusterClient;

    public SiloHealthcheck(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var mgmtGrain = _clusterClient.GetGrain<IManagementGrain>(0);

            // Vérifier que le silo répond en récupérant les statistiques de base
            var hosts = await mgmtGrain.GetHosts();

            if (hosts != null && hosts.Count > 0)
            {
                return HealthCheckResult.Healthy("Le silo Orleans est opérationnel");
            }
            else
            {
                return HealthCheckResult.Degraded("Le silo Orleans est démarré mais aucun hôte n'est actif");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Le silo Orleans n'est pas disponible", ex);
        }
    }
}
