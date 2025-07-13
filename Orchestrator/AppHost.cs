var builder = DistributedApplication.CreateBuilder(args);

var keyVault = builder.AddAzureKeyVault("my-api-secrets");

var api = builder.AddProject<Projects.Api>("api").WithReference(keyVault);

builder.AddProject<Projects.Proxy>("proxy").WithReference(api);

builder.Build().Run();