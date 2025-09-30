using SimpleVotingSystem.Polls;

namespace SimpleVotingSystem.Api.Polls.ListingPolls;

public record PollResult(Guid Id, string Libelle, PollOptionResult[] Options);
public record PollOptionResult(Guid Id, string Libelle, int Votes = 0);

public static class GetPollsEndpoint
{
    public static IEndpointRouteBuilder UseGetAllPollsEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("/", async (IGrainFactory grainFactory) =>
        {
            var pollGrain = grainFactory.GetGrain<IPollCatalogGrain>(Guid.Empty);
            var polls = await pollGrain.GetAllPolls();
            return polls.Select(p => new PollResult(p.Id, p.Libelle, p.Options.Select(option => new PollOptionResult(option.Id, option.Libelle)).ToArray()));
        });

        return endpoint;
    }
}