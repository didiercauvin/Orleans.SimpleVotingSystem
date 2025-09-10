using SimpleVotingSystem.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Api.Polls;

public class VoteForPollRequest
{
    public Guid OptionId { get; set; }
}

public static class VoteForPollEndPoint
{
    public static IEndpointRouteBuilder UseVoteForPollEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("{id}/vote", async (Guid id, VoteForPollRequest request, IGrainFactory grainFactory) =>
        {
            var pollGrain = grainFactory.GetGrain<IPollGrain>(id);

            await pollGrain.Vote(request.OptionId);
        });

        return endpoint;
    }
}