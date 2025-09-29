var builder = DistributedApplication.CreateBuilder(args);

var keyVault = builder.AddAzureKeyVault("my-api-secrets");


var postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin()
    .WithPgWeb()
    .WithDataVolume();
var postgresdb = postgres.AddDatabase("postgresdb");

var playwrightBrowser = builder
    .AddContainer("playwright-browser", "mcr.microsoft.com/playwright", "v1.54.0-noble")
    .WithContainerRuntimeArgs("--ipc=host", "--init", "--user", "pwuser")
    .WithHttpEndpoint(targetPort: 9222, name: "cdp")
    .WithEntrypoint("/bin/sh")
    // --- THIS IS THE FIX ---
    // Explicitly tell npx to use version 1.54.0 to match your client NuGet package.
    .WithArgs("-c", "npx playwright@1.54.0 run-server --port=9222 --host=0.0.0.0");

var api = builder
    .AddProject<Projects.Api>("api")
    .WithReference(keyVault)
    .WithEnvironment("Playwright__CdpEndpoint", playwrightBrowser.GetEndpoint("cdp"))
    .WithReference(postgresdb)
    .WaitFor(playwrightBrowser);

builder
    .AddProject<Projects.Proxy>("proxy")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();