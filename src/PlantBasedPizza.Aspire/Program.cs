var builder = DistributedApplication.CreateBuilder(args);

var db = builder
    .AddPostgres("database")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("plantbasedpizza");

var application = builder.AddProject<Projects.PlantBasedPizza_Api>("api")
    .WithReference(db)
    .WithEnvironment("ConnectionStrings__RecipesPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__OrderManagerPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__KitchenPostgresConnection", db)
    .WithEnvironment("ConnectionStrings__DeliveryPostgresConnection", db)
    .WaitFor(db);

builder.Build().Run();