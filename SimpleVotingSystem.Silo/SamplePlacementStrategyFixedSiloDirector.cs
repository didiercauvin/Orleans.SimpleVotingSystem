using Orleans.Runtime.Placement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Silo;

public class SamplePlacementStrategyFixedSiloDirector : IPlacementDirector
{
    public Task<SiloAddress> OnAddActivation(
        PlacementStrategy strategy,
        PlacementTarget target,
        IPlacementContext context)
    {
        var silos = context.GetCompatibleSilos(target).OrderBy(s => s).ToArray();
        int silo = GetSiloNumber(target.GrainIdentity.GetGuidKey(), silos.Length);

        return Task.FromResult(silos[silo]);
    }

    private int GetSiloNumber(Guid grainId, int silosCount)
    {
        // GrainId.PrimaryKey est un Guid
        // On en tire quelques bytes pour un hash simple et déterministe
        var bytes = grainId.ToByteArray();
        int hash = BitConverter.ToInt32(bytes, 0);

        // Valeur positive
        hash = (hash & 0x7fffffff);

        return hash % silosCount;
    }
}
