using SimpleVotingSystem.Api.Voters;

namespace SimpleVotingSystem.Api.Polls;

public static class VotersEndpoints
{
    public static IEndpointRouteBuilder UseVotersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("Voters");
        return group
                .UseVoteForPollEndpoint()
                .UseGetVoterPollsEndpoint();
    }
}
