using SimpleVotingSystem.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Api.Voters;

public class VoteForPollRequest
{
    public Guid PollId { get; set; }
    public Guid OptionId { get; set; }
}

public static class VoteForPollEndPoint
{
    public static IEndpointRouteBuilder UseVoteForPollEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("{id}/vote", async (string id, VoteForPollRequest request, IGrainFactory grainFactory) =>
        {
            var voterGrain = grainFactory.GetGrain<IVoterGrain>(id);
            var pollGrain = grainFactory.GetGrain<IPollGrain>(request.PollId);

            await voterGrain.VoteAsync(request.PollId, request.OptionId);
            await pollGrain.Vote(id, request.OptionId);
        });

        return endpoint;
    }
}