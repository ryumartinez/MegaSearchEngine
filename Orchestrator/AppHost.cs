var builder = DistributedApplication.CreateBuilder(args);

var keyVault = builder.AddAzureKeyVault("my-api-secrets");

var jaeger = builder.AddContainer("jaeger", "cr.jaegertracing.io/jaegertracing/jaeger", "latest")
    .WithHttpEndpoint(name: "jaeger-ui", targetPort: 16686)
    .WithEndpoint(name: "otlp", targetPort: 4317, scheme: "http");

var postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin();
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
    .WaitFor(playwrightBrowser)
    .WaitFor(postgresdb)
    .WaitFor(keyVault);

builder
    .AddProject<Projects.Proxy>("proxy")
    .WithReference(api)
    .WaitFor(api);

builder
    .AddProject<Projects.Scheduler>("scheduler")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();