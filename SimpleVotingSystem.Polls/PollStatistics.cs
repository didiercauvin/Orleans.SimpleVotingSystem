using Orleans.Placement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Polls;

public interface IPollStatisticsGrain : IGrainWithIntegerKey
{
    Task<PollStatistic[]> GetTotalVotes();
    Task<PollOption?> GetMostPopularOption();
}

[RandomPlacement]
public class PollStatisticsGrain : Grain, IPollStatisticsGrain
{
    public async Task<PollStatistic[]> GetTotalVotes()
    {
        var catalog = GrainFactory.GetGrain<IPollCatalogGrain>(0);
        var polls = await catalog.GetAllPolls();

        return polls.Select(p => new PollStatistic
        {
            Sondage = p.Libelle,
            TotalVotes = p.Options.Sum(o => o.Votes)
        }).ToArray();
    }

    public async Task<PollOption?> GetMostPopularOption()
    {
        var catalog = GrainFactory.GetGrain<IPollCatalogGrain>(0);
        var polls = await catalog.GetAllPolls();

        return polls.SelectMany(p => p.Options)
                    .OrderByDescending(o => o.Votes)
                    .FirstOrDefault();
    }
}

[GenerateSerializer]
public class PollStatistic
{
    [Id(0)]
    public string Sondage { get; set; }
    [Id(1)]
    public int TotalVotes { get; set; }
}