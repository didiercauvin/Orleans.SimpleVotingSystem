using SimpleVotingSystem.Api.Polls.ListingPolls;
using SimpleVotingSystem.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Api.Polls;

public static class ConsultPollResultsEndpoint
{
    public static IEndpointRouteBuilder UseConsultPollResultsEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("{id}/results", async (Guid id, IGrainFactory grainFactory) =>
        {
            var pollGrain = grainFactory.GetGrain<IPollGrain>(id);
            var results = await pollGrain.GetResults();
            return results.Select(option => new PollOptionResult(option.Id, option.Libelle, option.Votes));
        });

        return endpoint;
    }
}
