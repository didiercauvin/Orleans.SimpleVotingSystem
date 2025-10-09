using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Polls;

public interface IVoterGrain : IGrainWithStringKey
{
    Task<bool> VoteAsync(Guid pollId, Guid optionId);
    Task<VoteResult[]> GetVoteHistoryAsync();
}

public class VoterState
{
    public List<VoteRecord> Votes { get; set; } = new();
}

public class VoteRecord
{
    public Guid PollId { get; set; }
    public Guid OptionId { get; set; }
}

public class VoterGrain : Grain, IVoterGrain
{
    private readonly IPersistentState<VoterState> _state;

    public VoterGrain([PersistentState("voter", "pollStore")] IPersistentState<VoterState> state)
    {
        _state = state;
    }

    public async Task<bool> VoteAsync(Guid pollId, Guid optionId)
    {
        // Vérifie si ce votant a déjà voté
        if (_state.State.Votes.Any(v => v.PollId == pollId))
            return false;

        // Récupère le sondage
        var poll = GrainFactory.GetGrain<IPollGrain>(pollId);
        var pollData = await poll.GetPoll();
        var option = pollData.Options.FirstOrDefault(o => o.Id == optionId);
        if (option == null)
            return false;

        // Envoie le vote au sondage
        var success = await poll.Vote(this.GetPrimaryKeyString(), optionId);
        if (!success)
            return false;

        // Enregistre le vote dans l'état du voter
        _state.State.Votes.Add(new VoteRecord
        {
            PollId = pollId,
            OptionId = optionId
        });

        await _state.WriteStateAsync();
        return true;
    }

    public Task<VoteResult[]> GetVoteHistoryAsync()
    {
        var history = _state.State.Votes.Select(v => new VoteResult { PollId = v.PollId, OptionId = v.OptionId });
        return Task.FromResult(history.ToArray());
    }
}

[GenerateSerializer]
public class VoteResult
{
    [Id(0)] public Guid PollId { get; set; }
    [Id(1)] public Guid OptionId { get; set; }
}
