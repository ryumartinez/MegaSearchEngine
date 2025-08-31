var builder = DistributedApplication.CreateBuilder(args);

var keyVault = builder.AddAzureKeyVault("my-api-secrets");

var playwrightBrowser = builder
    .AddContainer("playwright-browser", "mcr.microsoft.com/playwright", "v1.55.0-noble")
    .WithContainerRuntimeArgs("--ipc=host", "--init", "--user", "pwuser")
    .WithHttpEndpoint(targetPort: 9222, name: "cdp")
    .WithEntrypoint("/bin/sh")
    .WithArgs("-c", "npx playwright run-server --port=9222 --host=0.0.0.0");

var api = builder
    .AddProject<Projects.Api>("api")
    .WithReference(keyVault)
    .WithEnvironment("Playwright__CdpEndpoint", playwrightBrowser.GetEndpoint("cdp"))
    .WaitFor(playwrightBrowser);

builder
    .AddProject<Projects.Proxy>("proxy")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();