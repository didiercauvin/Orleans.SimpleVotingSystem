using SimpleVotingSystem.Api.Polls.ListingPolls;
using SimpleVotingSystem.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Api.Voters;

public static class GetVoterPollsResultsEndpoint
{
    public static IEndpointRouteBuilder UseGetVoterPollsEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("{id}/results", async (string id, IGrainFactory grainFactory) =>
        {
            var voter = grainFactory.GetGrain<IVoterGrain>(id);
            var votes = await voter.GetVoteHistoryAsync();

            var tasks = votes.Select(async vote =>
            {
                var poll = grainFactory.GetGrain<IPollGrain>(vote.PollId);
                var pollData = await poll.GetPoll();
                var option = pollData.Options.FirstOrDefault(o => o.Id == vote.OptionId);

                return new VoteHistoryDto { Libelle = pollData.Libelle, Choix = option?.Libelle };
            });

            return await Task.WhenAll(tasks);
        });

        return endpoint;
    }
}

public class VoteHistoryDto
{
    public string Libelle { get; set; }
    public string Choix { get; set; }
}