var builder = DistributedApplication.CreateBuilder(args);

var keyVault = builder.AddAzureKeyVault("my-api-secrets");

var jaeger = builder.AddContainer("jaeger", "cr.jaegertracing.io/jaegertracing/jaeger", "latest")
    .WithHttpEndpoint(name: "jaeger-ui", targetPort: 16686)
    .WithEndpoint(name: "otlp", targetPort: 4317, scheme: "http");

var postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin();
var postgresdb = postgres.AddDatabase("postgresdb");
var tickerqdb = postgres.AddDatabase("tickerq-db");

var playwrightBrowser = builder
    .AddContainer("playwright-browser", "mcr.microsoft.com/playwright", "v1.54.0-noble")
    .WithContainerRuntimeArgs("--ipc=host", "--init", "--user", "pwuser")
    .WithHttpEndpoint(targetPort: 9222, name: "cdp")
    .WithEntrypoint("/bin/sh")
    // --- THIS IS THE FIX ---
    // Explicitly tell npx to use version 1.54.0 to match your client NuGet package.
    .WithArgs("-c", "npx playwright@1.54.0 run-server --port=9222 --host=0.0.0.0");

var openObserve = builder.AddContainer("openobserve", "public.ecr.aws/zinclabs/openobserve", "latest")
    .WithHttpEndpoint(targetPort: 5080, name: "ui")
    .WithEnvironment("ZO_ROOT_USER_EMAIL", "admin@example.com")
    .WithEnvironment("ZO_ROOT_USER_PASSWORD", "ComplexPass123")
    .WithBindMount("./openobserve-data", "/data"); // Persist your data

var openObserveEndpoint = openObserve.GetEndpoint("ui");

var api = builder
    .AddProject<Projects.Api>("api")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", $"{openObserve.GetEndpoint("ui")}/api/default") 
    .WithEnvironment("OTEL_EXPORTER_OTLP_HEADERS", "Authorization=Basic YWRtaW5AZXhhbXBsZS5jb206Q29tcGxleFBhc3MxMjM=")
    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf")
    .WithReference(tickerqdb)
    .WithReference(keyVault)
    .WithEnvironment("Playwright__CdpEndpoint", playwrightBrowser.GetEndpoint("cdp"))
    .WithReference(postgresdb)
    .WaitFor(openObserve)
    .WaitFor(playwrightBrowser)
    .WaitFor(postgresdb)
    .WaitFor(keyVault)
    .WaitFor(tickerqdb);

builder
    .AddProject<Projects.Proxy>("proxy")
    .WithReference(api)
    .WaitFor(api);

builder
    .AddProject<Projects.Scheduler>("scheduler")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();