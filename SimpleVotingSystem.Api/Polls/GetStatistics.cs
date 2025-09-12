using SimpleVotingSystem.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Api.Polls;

public static class GetStatisticsEndpoint
{
    public static IEndpointRouteBuilder UseGetStatisticsEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("statistics", async (IGrainFactory grainFactory) =>
        {
            var grain = grainFactory.GetGrain<IPollStatisticsGrain>(0);
            var stat = await grain.GetTotalVotes();
            return Results.Ok(stat);
        });

        return endpoint;
    }
}