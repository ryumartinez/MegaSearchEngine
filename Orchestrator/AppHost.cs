var builder = DistributedApplication.CreateBuilder(args);

var keyVault = builder.AddAzureKeyVault("key-vault");

builder.AddProject<Projects.Api>("api")
    .WithReference(keyVault);

builder.Build().Run();