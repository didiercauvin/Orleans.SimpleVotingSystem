using Orleans.Placement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Polls;

[Serializable]
public sealed class SamplePlacementStrategy : PlacementStrategy
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class SamplePlacementStrategyAttribute : PlacementAttribute
{
    public SamplePlacementStrategyAttribute() :
        base(new SamplePlacementStrategy())
    {
    }
}
