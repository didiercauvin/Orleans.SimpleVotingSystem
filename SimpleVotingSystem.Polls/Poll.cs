using Orleans.Streams;

namespace SimpleVotingSystem.Polls;

public interface IPollGrain : IGrainWithGuidKey
{
    Task CreatePoll(Poll poll);
    //Task<bool> Vote(string voterId, Guid optionId);
    Task<PollOption[]> GetResults();
    Task<Poll> GetPoll();
}

[GenerateSerializer]
public record PollCreatedEvent
{
    [Id(0)]
    public Guid PollId { get; set; }
}

[SamplePlacementStrategy]
public class PollGrain : Grain, IPollGrain
{
    private readonly IPersistentState<PollState> _state;
    private StreamSubscriptionHandle<VoteCastEvent>? _subscription;

    public PollGrain([PersistentState("poll", "pollStore")] IPersistentState<PollState> state)
    {
        _state = state;
    }

    public override async Task OnActivateAsync(CancellationToken ct)
    {
        var streamProvider = this.GetStreamProvider("votes-stream");

        var streamId = StreamId.Create("VoteStream", this.GetPrimaryKey().ToString());
        // Le poll a une clé Guid, donc on utilise GetPrimaryKey()
        var stream = streamProvider.GetStream<VoteCastEvent>(streamId);

        _subscription = await stream.SubscribeAsync(OnVoteReceived);
    }

    private async Task OnVoteReceived(VoteCastEvent vote, StreamSequenceToken? token = null)
    {
        await Vote(vote.VoterId, vote.OptionId);
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
    }

    private async Task<bool> Vote(string voterId, Guid optionId)
    {
        var option = _state.State.Poll.Options.FirstOrDefault(o => o.Id == optionId);
        if (option == null)
            return false;

        option.Votes++;
        _state.State.Voters.Add(voterId);

        await _state.WriteStateAsync();
        return true;
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
    public HashSet<string> Voters { get; set; } = new();

}

public class PollOptionState
{
    public Guid Id { get; set; }
    public string? Libelle { get; set; }
}
