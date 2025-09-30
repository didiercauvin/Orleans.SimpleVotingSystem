using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.Hosting;
using SimpleVotingSystem.Polls;
using SimpleVotingSystem.Silo;

//using var host = Host.CreateDefaultBuilder(args)
//    .UseOrleans(siloBuilder =>
//    {
//        siloBuilder.UseLocalhostClustering();
//        siloBuilder.UseDashboard();

//        siloBuilder.AddMemoryGrainStorage("pollStore");
//    }).Build();

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
            app.UseOrleansDashboard();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksWithJsonResponse("/health");
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
        var connectionString = context.Configuration.GetConnectionString("SondageAppCluster") ?? "Server=localhost;Database=SondageAppCluster;Integrated Security=true;TrustServerCertificate=True";

        //siloBuilder.UseAzureStorageClustering(options => options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true"));

        //siloBuilder.UseLocalhostClustering(clusterId: "default", serviceId: "default");
        siloBuilder.UseAdoNetClustering(options =>
        {
            options.Invariant = "System.Data.SqlClient"; // Pour SQL Server
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

        siloBuilder.UseDashboard(options =>
        {
            //options.Port = int.Parse(Environment.GetEnvironmentVariable("DASHBOARD_PORT") ?? "0");
            options.Host = "*";
            options.HostSelf = true;
            options.CounterUpdateIntervalMs = 5000;
        });

        siloBuilder.AddAdoNetGrainStorage("pollStore", options =>
        {
            options.Invariant = "System.Data.SqlClient"; // ou "Microsoft.Data.SqlClient"
            options.ConnectionString = connectionString;
        });

    })
    .Build();

// Start the host
await host.StartAsync();

Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();
