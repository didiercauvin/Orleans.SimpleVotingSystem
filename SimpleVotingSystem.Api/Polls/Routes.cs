using SimpleVotingSystem.Api.Polls.ListingPolls;

namespace SimpleVotingSystem.Api.Polls;

public static class PollEndpoints
{
    public static IEndpointRouteBuilder UsePollEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("Polls");
        return group
                .UseGetPollEndpoint()
                .UseConsultPollResultsEndpoint()
                .UseGetAllPollsEndpoint()
                .UseCreatePollEndpoint()
                .UseVoteForPollEndpoint();
    }
}
