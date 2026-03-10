var builder = DistributedApplication.CreateBuilder(args);

var clusterConnection = builder.AddConnectionString("SondageAppCluster");

var silo1 = builder.AddProject<Projects.SimpleVotingSystem_Silo>("silo-1")
    .WithReference(clusterConnection)
    .WithEnvironment("ORLEANS_SILO_PORT", "11111")
    .WithEnvironment("ORLEANS_GATEWAY_PORT", "30001")
    .WithEnvironment("SILO_NAME", "JohnDoe")
    .WithEndpoint("http", e => e.Port = 5000)
    .WithEndpoint("https", e => e.Port = 7000);

var silo2 = builder.AddProject<Projects.SimpleVotingSystem_Silo>("silo-2")
    .WithReference(clusterConnection)
    .WithEnvironment("ORLEANS_SILO_PORT", "11112")
    .WithEnvironment("ORLEANS_GATEWAY_PORT", "30002")
    .WithEnvironment("SILO_NAME", "JohnSnow")
    .WithEndpoint("http", e => e.Port = 5001)
    .WithEndpoint("https", e => e.Port = 7001);

var silo3 = builder.AddProject<Projects.SimpleVotingSystem_Silo>("silo-3")
    .WithReference(clusterConnection)
    .WithEnvironment("ORLEANS_SILO_PORT", "11113")
    .WithEnvironment("ORLEANS_GATEWAY_PORT", "30003")
    .WithEnvironment("SILO_NAME", "Superman")
    .WithEndpoint("http", e => e.Port = 5002)
    .WithEndpoint("https", e => e.Port = 7002);

builder.AddProject<Projects.SimpleVotingSystem_Api>("simplevotingsystem-api")
    .WithReference(clusterConnection)
    .WaitFor(silo1)
    .WaitFor(silo2)
    .WaitFor(silo3);

builder.Build().Run();
