

# Infrastructure Overview

This project uses **.NET Aspire** to orchestrate distributed services. Aspire provides a code-first approach to service discovery, configuration, and observability.

## 🏗️ Architecture

We use a "Hub and Spoke" model where the **AppHost** (Orchestrator) manages the lifecycle of all containers and projects.

### Core Components

| Component | Technology | Description |
| --- | --- | --- |
| **Orchestrator** | .NET Aspire 13 | Manages service discovery and environment variables. |
| **Telemetry** | OpenObserve | Persistent storage for Logs, Metrics, and Traces (OTLP). |
| **Database** | PostgreSQL | Primary data store for the API and Scheduler. |
| **Scraper** | Playwright | Headless browser container for data extraction. |

---

## 🚀 Publishing to Docker Compose

Instead of manually writing a `docker-compose.yml`, we generate it directly from our C# `AppHost` definition. This ensures that our local development environment and our production deployment are always in sync.

### 1. Prerequisites

Ensure you have the Aspire CLI installed on your machine:

```bash
dotnet tool install -g aspire.cli

```

### 2. Enable the Docker Publisher

In the `Orchestrator/Program.cs`, ensure the Docker Compose environment is defined:

```csharp
// This line enables the 'aspire publish' command to target Docker Compose
builder.AddDockerComposeEnvironment("mega-search-env");

```

### 3. Generate Deployment Artifacts

Run the following command from the root of the solution to generate the manifest and the Compose files:

```bash
aspire publish -o ./publish-artifacts

```

This command produces:

* `docker-compose.yaml`: The full orchestration file.
* `.env`: A template for secrets (passwords, API keys) that must be filled before deployment.
* Container images for your local projects (`Api`, `Proxy`, etc.).

### 4. Running the Stack

Once the artifacts are generated, you can launch the entire infrastructure on any Docker-compatible host (like your local machine or a **Portainer** instance):

```bash
cd publish-artifacts
# Fill in the secrets in the .env file first!
docker compose up -d

```

---

## 🛠️ Infrastructure Maintenance

* **Adding a New Service:** Add the project to the `Orchestrator` using `builder.AddProject<T>()`.
* **Updating Versions:** Update the `Directory.Packages.props` file in the root. The `aspire publish` command will pick up the changes automatically.
* **Environment Variables:** All cross-service communication is handled via **Service Discovery**. Do not hardcode URLs; use `.WithReference(service)` in the `AppHost`.

---

### Tips for your Rider Setup

1. **Solution Items:** Since you created this in a documentation project, you can right-click the file in Rider and select **Open in Browser** to see the rendered version.
2. **Mermaid Diagrams:** If you want to add a visual graph of your services, you can add a `mermaid` block to this file, and Rider's Markdown preview will render it automatically.

**Would you like me to show you how to add a "Deployment Script" to your solution that automates the `aspire publish` and `docker compose up` steps into a single command?**