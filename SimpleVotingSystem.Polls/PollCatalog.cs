using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Polls;

public interface IPollCatalogGrain : IGrainWithIntegerKey
{
    Task RegisterPoll(Guid pollId);
    Task<List<Poll>> GetAllPolls();
}

public class PollCatalogGrain : Grain, IPollCatalogGrain
{
    private readonly IPersistentState<List<Guid>> _pollIds;

    public PollCatalogGrain(
        [PersistentState("pollCatalog", "pollStore")] IPersistentState<List<Guid>> pollIds)
    {
        _pollIds = pollIds;
    }

    public async Task RegisterPoll(Guid pollId)
    {
        if (!_pollIds.State.Contains(pollId))
        {
            _pollIds.State.Add(pollId);
            await _pollIds.WriteStateAsync();
        }
    }

    public async Task<List<Poll>> GetAllPolls()
    {
        // Charger tous les grains correspondants
        var tasks = _pollIds.State
            .Select(id => GrainFactory.GetGrain<IPollGrain>(id).GetPoll())
            .ToList();

        return (await Task.WhenAll(tasks)).ToList();
    }
}
