using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(config =>
    {
        config.Configure(app =>
        {
            app.UseRouting();
            app.UseOrleansDashboard();
        });

    })
    .UseOrleans((context, siloBuilder) =>
    {
        siloBuilder.UseLocalhostClustering();

        siloBuilder.UseDashboard(options =>
        {
            //options.Port = int.Parse(Environment.GetEnvironmentVariable("DASHBOARD_PORT") ?? "0");
            options.Host = "*";
            options.HostSelf = true;
            options.CounterUpdateIntervalMs = 5000;
        });

    })
    .Build();

// Start the host
await host.StartAsync();

Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();