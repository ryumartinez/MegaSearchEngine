var builder = DistributedApplication.CreateBuilder(args);

var existingKeyVaultName = builder.AddParameter("existingKeyVaultName");
var existingKeyVaultResourceGroup = builder.AddParameter("existingKeyVaultResourceGroup");

var keyVault = builder.AddAzureKeyVault("keyVault")
    .AsExisting(existingKeyVaultName, existingKeyVaultResourceGroup);

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(keyVault);

builder.AddProject<Projects.Proxy>("proxy").WithReference(api);

builder.Build().Run();