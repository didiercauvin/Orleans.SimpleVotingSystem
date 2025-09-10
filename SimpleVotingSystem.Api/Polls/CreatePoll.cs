using SimpleVotingSystem.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Api.Polls;

public class CreatePollRequest
{
    public string? Libelle { get; set; }
    public CreatePollOptionRequest[] Options { get; set; } = [];
}

public class CreatePollOptionRequest
{
    public Guid Id { get; set; }
    public string? Libelle { get; set; }
}

public static class CreatePollEndPoint
{
    public static IEndpointRouteBuilder UseCreatePollEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("{id}", async (Guid id, CreatePollRequest request, IGrainFactory grainFactory) =>
        {
            var pollGrain = grainFactory.GetGrain<IPollGrain>(id);

            await pollGrain.CreatePoll(
                new Poll
                {
                    Id = id,
                    Libelle = request.Libelle,
                    Options = request.Options.Select(o => new PollOption { Id = o.Id, Libelle = o.Libelle }).ToArray()
                });
        });

        return endpoint;
    }
}
