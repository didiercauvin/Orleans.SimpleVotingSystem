using Microsoft.Extensions.Options;
using Spectre.Console;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Color = Spectre.Console.Color;

var http = new HttpClient { BaseAddress = new Uri("https://localhost:7150") };

AnsiConsole.Write(
    new FigletText("Elap Vote")
        .Centered()
        .Color(Color.DarkCyan));

while (true)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green]Que veux-tu faire ?[/]")
            .AddChoices("Créer un sondage", "Voter", "Voir résultats", "Quitter"));

    switch (choice)
    {
        case "Créer un sondage":
            await CreatePoll(http);
            break;

        case "Voter":
            await VoteForPoll(http);
            break;

        case "Voir résultats":
            await ConsultPollResults(http);
            break;

        case "Quitter":
            return;
    }
}

async Task ConsultResults(Sondage sondage)
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

async Task Voter(Sondage sondage)
{
    var choix = AnsiConsole.Prompt(
        new SelectionPrompt<SondageOption>()
            .Title($"[yellow]Options pour {sondage.Libelle}[/]")
            .UseConverter(o => o.Libelle!)
            .AddChoices(sondage.Options));

    var vote = new VoteForPollDto { OptionId = choix.Id };

    await http.PostAsJsonAsync<VoteForPollDto>(
                $"/polls/{sondage.Id}/vote", vote, MyJsonContext.Default.VoteForPollDto);

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

async Task VoteForPoll(HttpClient http)
{
    var sondages = await http.GetFromJsonAsync<List<Sondage>>("/polls", MyJsonContext.Default.ListSondage);
    var choixSondages = AnsiConsole.Prompt(
        new SelectionPrompt<Sondage>()
            .Title("[green]Sondages disponibles:[/]")
            .UseConverter(s => s.Libelle!)
            .AddChoices(sondages!));

    Voter(choixSondages);
}

async Task ConsultPollResults(HttpClient http)
{
    var currentSondages = await http.GetFromJsonAsync<List<Sondage>>("/polls", MyJsonContext.Default.ListSondage);
    var resultSondage = AnsiConsole.Prompt(
        new SelectionPrompt<Sondage>()
            .Title("[green]Sondages disponibles:[/]")
            .UseConverter(s => s.Libelle!)
            .AddChoices(currentSondages!));

    await ConsultResults(resultSondage);
}

public class VoteForPollDto
{
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

[JsonSerializable(typeof(Sondage))]
[JsonSerializable(typeof(List<Sondage>))]
[JsonSerializable(typeof(SondageOption))]
[JsonSerializable(typeof(SondageOption[]))]
[JsonSerializable(typeof(VoteForPollDto))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class MyJsonContext : JsonSerializerContext
{
}
