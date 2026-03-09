var builder = DistributedApplication.CreateBuilder(args);

var orleans = builder.AddOrleans("cluster");

builder.AddProject<Projects.SimpleVotingSystem_Silo>("simplevotingsystem-silo")
    .WithReference(orleans);

builder.Build().Run();
