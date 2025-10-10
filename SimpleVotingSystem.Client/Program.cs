using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spectre.Console;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Color = Spectre.Console.Color;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var sondageApiUrl = config.GetRequiredSection("VotingApi").GetValue<string>("Url");

var http = new HttpClient { BaseAddress = new Uri(sondageApiUrl) };

AnsiConsole.Write(
    new FigletText("Elap Vote")
        .Centered()
        .Color(Color.DarkCyan));

string voterIdFile = "voter.id";
string voterId;

if (!File.Exists(voterIdFile))
{
    Console.Write("Entrez votre identifiant : ");
    voterId = Console.ReadLine()?.Trim() ?? Guid.NewGuid().ToString();
    File.WriteAllText(voterIdFile, voterId);
}
else
{

    voterId = File.ReadAllText(voterIdFile).Trim();
}

    while (true)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]Que veux-tu faire ?[/]")
                .AddChoices("Créer un sondage", "Voter", "Voir mes votes", "Voir résultats", "Voir les stats globales", "Quitter"));

        switch (choice)
        {
            case "Créer un sondage":
                await CreatePoll(http);
                break;

            case "Voter":
                await VoteForPoll(http, voterId);
                break;

            case "Voir résultats":
                await ConsultPollResults(http);
                break;

            case "Voir mes votes":
                await ConsultMyPolls(http, voterId);
                break;

            case "Voir les stats globales":
                await ViewStatistics(http);
                break;

            case "Quitter":
                return;
        }
    }

async Task ViewStatistics(HttpClient http)
{
    var results = await http.GetFromJsonAsync<StatSondage[]>($"/polls/statistics", MyJsonContext.Default.StatSondageArray);

    var table = new Table();
    table.AddColumn("[yellow]Sondage[/]");
    table.AddColumn("[cyan]Nb. votes[/]");

    foreach (var r in results)
    {
        table.AddRow(r.Sondage, r.TotalVotes.ToString());
    }

    AnsiConsole.Write(table);
}

async Task ConsultResults(HttpClient http, Sondage sondage)
{
    var results = await http.GetFromJsonAsync<SondageOption[]>($"/polls/{sondage.Id}/results", MyJsonContext.Default.SondageOptionArray);

    var table = new Table();
    table.AddColumn("[yellow]Option[/]");
    table.AddColumn("[cyan]Votes[/]");

    foreach (var r in results)
    {
        table.AddRow(r.Libelle, r.Votes.ToString());
    }

    AnsiConsole.Write(table);
}

async Task ConsultMyPolls(HttpClient http, string voterId)
{
    var results = await http.GetFromJsonAsync<MonVote[]>($"/voters/{voterId}/results", MyJsonContext.Default.MonVoteArray);

    var table = new Table();
    table.AddColumn("[yellow]Sondage[/]");
    table.AddColumn("[cyan]Choix[/]");

    foreach (var r in results)
    {
        table.AddRow(r.Libelle, r.Choix);
    }

    AnsiConsole.Write(table);
}

async Task Voter(string voterId, Sondage sondage)
{
    var choix = AnsiConsole.Prompt(
        new SelectionPrompt<SondageOption>()
            .Title($"[yellow]Options pour {sondage.Libelle}[/]")
            .UseConverter(o => o.Libelle!)
            .AddChoices(sondage.Options));

    var vote = new VoteForPollDto { PollId = sondage.Id, OptionId = choix.Id };

    await http.PostAsJsonAsync<VoteForPollDto>(
                $"/voters/{voterId}/vote", vote, MyJsonContext.Default.VoteForPollDto);

    AnsiConsole.MarkupLine($"Vous avez voté pour : [cyan]{choix.Libelle}[/] dans le sondage [green]{sondage.Libelle}[/]");
}

static async Task CreatePoll(HttpClient http)
{
    var question = AnsiConsole.Ask<string>("Question du sondage :");
    var options = AnsiConsole.Ask<string>("Options (séparées par des virgules) :")
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(o => o.Trim()).ToArray();

    var pollId = Guid.NewGuid();

    await http.PostAsJsonAsync<Sondage>(
        $"/polls/{pollId}",
        new Sondage
        {
            Id = pollId,
            Libelle = question,
            Options = options.Select(option => new SondageOption { Id = Guid.NewGuid(), Libelle = option }).ToArray()
        }, MyJsonContext.Default.Sondage);

    AnsiConsole.MarkupLine($"[bold green]Sondage créé ![/] ID : [yellow]{pollId}[/]");
}

async Task VoteForPoll(HttpClient http, string voterId)
{
    var sondages = await http.GetFromJsonAsync<List<Sondage>>("/polls", MyJsonContext.Default.ListSondage);

    if (sondages == null || sondages.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]Aucun sondage en ligne pour le moment.[/]");
        return;
    }

    var choixSondages = AnsiConsole.Prompt(
        new SelectionPrompt<Sondage>()
            .Title("[green]Sondages disponibles:[/]")
            .UseConverter(s => s.Libelle!)
            .AddChoices(sondages!));

    await Voter(voterId, choixSondages);
}

async Task ConsultPollResults(HttpClient http)
{
    var currentSondages = await http.GetFromJsonAsync<List<Sondage>>("/polls", MyJsonContext.Default.ListSondage);

    if (currentSondages == null || currentSondages.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]Aucun sondage en ligne pour le moment.[/]");
        return;
    }

    var resultSondage = AnsiConsole.Prompt(
        new SelectionPrompt<Sondage>()
            .Title("[green]Sondages disponibles:[/]")
            .UseConverter(s => s.Libelle!)
            .AddChoices(currentSondages));

    await ConsultResults(http, resultSondage);
}

public class VoteForPollDto
{
    public Guid PollId { get; set; }
    public Guid OptionId { get; set; }
}

public class Sondage
{
    public Guid Id { get; set; }
    public string? Libelle { get; set; }
    public SondageOption[] Options { get; set; } = [];
}

public class SondageOption
{
    public Guid Id { get; set; }
    public string? Libelle { get; set; }
    public int Votes { get; set; } = 0;
}

public class MonVote
{
    public string Libelle { get; set; }
    public string Choix { get; set; }
}

public class StatSondage
{
    public string Sondage { get; set; }
    public int TotalVotes { get; set; }
}

[JsonSerializable(typeof(Sondage))]
[JsonSerializable(typeof(List<Sondage>))]
[JsonSerializable(typeof(SondageOption))]
[JsonSerializable(typeof(SondageOption[]))]
[JsonSerializable(typeof(VoteForPollDto))]
[JsonSerializable(typeof(StatSondage))]
[JsonSerializable(typeof(StatSondage[]))]
[JsonSerializable(typeof(MonVote))]
[JsonSerializable(typeof(MonVote[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class MyJsonContext : JsonSerializerContext
{
}
