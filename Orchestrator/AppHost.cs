var builder = DistributedApplication.CreateBuilder(args);

var keyVault = builder.AddAzureKeyVault("my-api-secrets");

// Add the Playwright container with the corrected arguments
var playwrightBrowser = builder.AddContainer("playwright-browser", "mcr.microsoft.com/playwright", "v1.55.0-noble")
    // Pass ALL Docker runtime arguments here
    .WithContainerRuntimeArgs("--ipc=host", "--init", "--user", "pwuser")
    // Expose the Chrome DevTools Protocol (CDP) port
    .WithHttpEndpoint(targetPort: 9222, name: "cdp")
    // Set the container's entrypoint to the shell
    .WithEntrypoint("/bin/sh")
    // Pass the command to run as an argument to the shell (without the --browser flag)
    .WithArgs("-c", "npx playwright run-server --port=9222 --host=0.0.0.0");


var api = builder.AddProject<Projects.Api>("api")
    .WithReference(keyVault).WithReference(playwrightBrowser.GetEndpoint("cdp"));

builder.AddProject<Projects.Proxy>("proxy").WithReference(api);

builder.Build().Run();