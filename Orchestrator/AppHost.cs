using Azure.Identity;

var builder = DistributedApplication.CreateBuilder(args);

var existingKeyVaultName = builder.AddParameter("existingKeyVaultName");
var existingKeyVaultResourceGroup = builder.AddParameter("existingKeyVaultResourceGroup");

var keyVault = builder.AddAzureKeyVault("keyVault")
    .AsExisting(existingKeyVaultName, existingKeyVaultResourceGroup);

builder.AddProject<Projects.Api>("api")
    .WithReference(keyVault);

builder.Build().Run();