using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.Dashboard;
using SimpleVotingSystem.Polls;
using SimpleVotingSystem.Silo;

var siloPort = int.Parse(Environment.GetEnvironmentVariable("ORLEANS_SILO_PORT") ?? "11111");
var gatewayPort = int.Parse(Environment.GetEnvironmentVariable("ORLEANS_GATEWAY_PORT") ?? "30001");
var siloName = Environment.GetEnvironmentVariable("SILO_NAME") ?? "JohnDoe";

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
        if (context.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<Program>();
        }
    })
    .ConfigureWebHostDefaults(config =>
    {
        config.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksWithJsonResponse("/health");
                endpoints.MapOrleansDashboard();
            });
        });

        config.ConfigureServices(services =>
        {
            services.AddPlacementDirector<SamplePlacementStrategy, SamplePlacementStrategyFixedSiloDirector>();
            services.AddRouting();
            services.AddHealthChecks().AddCheck<SiloHealthcheck>("silo");
        });
    })
    .UseOrleans((context, siloBuilder) =>
    {
        var connectionString = context.Configuration.GetConnectionString("SondageAppCluster") ?? throw new InvalidOperationException("Connection string introuvable");

        //siloBuilder.UseLocalhostClustering(clusterId: "default", serviceId: "default");
        siloBuilder.UseAdoNetClustering(options =>
        {
            options.Invariant = "Microsoft.Data.SqlClient"; // Pour SQL Server
            options.ConnectionString = connectionString;
        });

        siloBuilder.Configure<SiloOptions>(options =>
        {
            options.SiloName = siloName;
        });

        siloBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "sondage-app-orleans";
            options.ServiceId = "sondage-app-orleans";
        });

        siloBuilder.Configure<EndpointOptions>(options =>
        {
            options.SiloPort = siloPort;
            options.GatewayPort = gatewayPort;
        });

        siloBuilder.AddDashboard(options =>
        {
            //options.Port = int.Parse(Environment.GetEnvironmentVariable("DASHBOARD_PORT") ?? "0");
            //options.Host = "*";
            //options.HostSelf = true;
            options.CounterUpdateIntervalMs = 5000;
        });

        siloBuilder
            .AddMemoryGrainStorage("pollStore")
            .AddMemoryGrainStorage("PubSubStore")
            .AddMemoryStreams("votes-stream");

        siloBuilder.AddAdoNetGrainStorage("pollStore", options =>
        {
            options.Invariant = "Microsoft.Data.SqlClient"; // ou "Microsoft.Data.SqlClient"
            options.ConnectionString = connectionString;
        });


    })
    .Build();

//var builder = Host.CreateApplicationBuilder(args);

//// Aspire service defaults
//builder.AddServiceDefaults();

//// Orleans sans configuration manuelle — tout vient de l'AppHost
//builder.UseOrleans(silo =>
//{
//    var connectionString = builder.Configuration.GetConnectionString("SondageAppCluster")
//        ?? throw new InvalidOperationException("Connection string introuvable");

//    // Force explicitement au lieu de laisser Aspire détecter
//    silo.UseAdoNetClustering(options =>
//    {
//        options.Invariant = "Microsoft.Data.SqlClient";
//        options.ConnectionString = connectionString;
//    });

//    silo.Configure<ClusterOptions>(options =>
//    {
//        options.ClusterId = "sondage-app-orleans";
//        options.ServiceId = "sondage-app-orleans";
//    });

//    silo.Configure<EndpointOptions>(options =>
//    {
//        options.SiloPort = GetAvailablePort();
//        options.GatewayPort = GetAvailablePort();
//    });

//    silo
//        .AddMemoryGrainStorage("pollStore")
//        .AddMemoryGrainStorage("PubSubStore")
//        .AddMemoryStreams("votes-stream");

//    silo.AddAdoNetGrainStorage("pollStore", options =>
//    {
//        options.Invariant = "Microsoft.Data.SqlClient"; // ou "Microsoft.Data.SqlClient"
//        options.ConnectionString = connectionString;
//    });
//});

//// Tes services custom
//builder.Services.AddHealthChecks().AddCheck<SiloHealthcheck>("silo");

//using var host = builder.Build();

// Start the host
await host.StartAsync();

Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();
