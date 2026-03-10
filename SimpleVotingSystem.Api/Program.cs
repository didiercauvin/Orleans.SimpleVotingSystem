using Orleans.Configuration;
using SimpleVotingSystem.Api.Polls;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("SondageAppCluster");

// Add services to the container.

builder.Services.AddControllers();

builder.Services
    .AddOrleansClient(clientBuilder =>
    {
        //clientBuilder.UseAzureStorageClustering(options => options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true"));

        // Tell the client how to connect to Orleans (you'll need to customize this for yourself)
        //clientBuilder.UseLocalhostClustering();
        clientBuilder.UseAdoNetClustering(options =>
        {
            options.Invariant = "Microsoft.Data.SqlClient"; // Pour SQL Server
            options.ConnectionString = connectionString;
        });

        clientBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "sondage-app-orleans";
            options.ServiceId = "sondage-app-orleans";
        });

        //clientBuilder.AddMemoryStreams("votes-stream");
    });

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UsePollEndpoints();
app.UseVotersEndpoints();

app.Run();
