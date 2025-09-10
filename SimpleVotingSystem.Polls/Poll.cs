namespace SimpleVotingSystem.Polls;

public interface IPollGrain : IGrainWithGuidKey
{
    Task CreatePoll(Poll poll);
    Task Vote(Guid optionId);
    Task<PollOption[]> GetResults();
    Task<Poll> GetPoll();
}

public class PollGrain : Grain, IPollGrain
{
    private readonly IPersistentState<PollState> _state;

    public PollGrain([PersistentState("poll", "pollStore")] IPersistentState<PollState> state)
    {
        _state = state;
    }

    public async Task CreatePoll(Poll poll)
    {
        if (_state.RecordExists)
            throw new InvalidOperationException("Un sondage existe déjà pour ce grain.");

        _state.State = new PollState
        {
            Poll = poll,
            Options = poll.Options.ToArray()
        };

        await _state.WriteStateAsync();

        // Enregistrer ce sondage dans le catalogue
        await GrainFactory.GetGrain<IPollCatalogGrain>(0).RegisterPoll(this.GetPrimaryKey());
    }

    public async Task Vote(Guid optionId)
    {
        var option = _state.State.Poll.Options.FirstOrDefault(o => o.Id == optionId);
        option.Votes++;

        await _state.WriteStateAsync();
    }

    public Task<Poll> GetPoll()
    {
        return Task.FromResult(_state.State.Poll);
    }


    public Task<PollOption[]> GetResults()
    {
        return Task.FromResult(_state.State.Options);
    }
}

[GenerateSerializer]
public class Poll
{
    [Id(0)]
    public Guid Id { get; set; }
    [Id(1)]
    public string? Libelle { get; set; }
    [Id(2)]
    public PollOption[] Options { get; set; } = [];
}

[GenerateSerializer]
public class PollOption
{
    [Id(0)]
    public Guid Id { get; set; }
    [Id(1)]
    public string? Libelle { get; set; }
    [Id(2)]
    public int Votes { get; set; } = 0;
}

public class PollState
{
    public Poll? Poll { get; set; }
    public PollOption[] Options { get; set; } = [];

}

public class PollOptionState
{
    public Guid Id { get; set; }
    public string? Libelle { get; set; }
}
