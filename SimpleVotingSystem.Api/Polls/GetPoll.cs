using SimpleVotingSystem.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Api.Polls;

public static class GetPollEndpoint
{
    public static IEndpointRouteBuilder UseGetPollEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("{id}", async (Guid id, IGrainFactory grainFactory) =>
        {
            var pollGrain = grainFactory.GetGrain<IPollGrain>(id);
            var poll = await pollGrain.GetPoll();
            return Results.Ok(poll);
        });

        return endpoint;
    }
}
