using SimpleVotingSystem.Api.Polls;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Host.UseOrleansClient(static builder =>
{
    builder.UseLocalhostClustering();
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UsePollEndpoints();

app.Run();
