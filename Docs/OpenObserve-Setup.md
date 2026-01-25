Pre-built dashboards are the most efficient way to get immediate value out of OpenObserve without writing complex SQL or PromQL from scratch. Since your current architecture includes **PostgreSQL**, **.NET 10**, and **Playwright**, you can leverage existing templates to monitor these specific components.

Here is a guide on how to find, import, and adapt them for your environment.

### 1. Where to Find the Templates

OpenObserve supports standard JSON templates. You can find high-quality pre-built dashboards in two main places:

* **OpenObserve GitHub Repository:** Look in the `dashboards` folder for official templates for Linux system metrics, PostgreSQL, and basic OTLP traces.
* **Grafana Dashboards:** Most OpenObserve panels are compatible with Grafana JSON exports. You can browse the [Grafana Labs Dashboard Gallery](https://grafana.com/grafana/dashboards/) for "PostgreSQL" or "OpenTelemetry DotNet" and download the JSON file.

### 2. Importing the JSON to OpenObserve

Once you have a JSON file, follow these steps:

1. Open the OpenObserve UI at `http://localhost:5080`.
2. On the left sidebar, click the **Dashboards** icon.
3. Click the **Import** button at the top right of the screen.
4. Either **Upload** the JSON file or **Paste** the JSON content directly into the text area.
5. Click **Import**. The dashboard will now appear in your list.

### 3. Mapping Streams to Your Data

Pre-built dashboards often use generic stream names (like `default`). Because your .NET 10 environment sends data to specific streams (like `api` or `DataAccess.Playwright`), you may need to update the queries:

1. Open the imported dashboard.
2. Click the **Settings** (cog icon) in the top right and go to **Variables**.
3. Look for a variable named `stream` or `org`. Update its query to point to your actual organization and stream name so the dropdowns work correctly.
4. If a specific chart is empty, click the **Edit** (pencil icon) on that panel. Update the `FROM` clause in the SQL to match your stream:
* *Change:* `FROM "default"`
* *To:* `FROM "api"` (or whatever stream your .NET app is logging to).



### 4. Essential Pre-Built Dashboards for your Setup

#### PostgreSQL Dashboard

Since you are running Postgres in a container via the .NET Aspire `AddPostgres` method, you should import a Postgres dashboard.

* **What it tracks:** Connection counts, cache hit ratios, and slow queries.
* **Setup:** Ensure you have the `Postgres` exporter enabled (standard in Aspire 13) so the metrics are actually flowing into OpenObserve.

#### .NET Runtime Dashboard

Look for an "OTLP Runtime Metrics" template.

* **What it tracks:** GC (Garbage Collection) heap sizes, ThreadPool threads, and CPU usage specifically for your API and Proxy projects.
* **Benefit:** This is critical for seeing if your Playwright browser instances are causing memory pressure on your local machine.

#### Playwright / Tracing Dashboard

Instead of a generic dashboard, look for the **"OpenTelemetry Trace List"** pre-built view.

* **Usage:** This dashboard allows you to filter specifically by the `DataAccess.Playwright` source.
* **Optimization:** You can add a filter at the top for `operation_name` to isolate your "Search" or "Scrape" actions and see their average duration over time.

### 5. Managing Dashboards via the CLI

If you want to automate this as part of your solution setup, you can use the OpenObserve API to "push" these dashboards during your deployment or startup.

```bash
# Example of pushing a dashboard JSON via curl
curl -u admin@example.com:ComplexPass123 \
     -X POST http://localhost:5080/api/default/dashboards \
     -H "Content-Type: application/json" \
     -d @my-postgres-dashboard.json

```

### 6. Pro-Tip: Setting the Time Zone

Since you are working in **Asunción (GMT-3)**, ensure your OpenObserve profile is set to the correct time zone.

* Click your **User Profile** icon in the bottom left.
* Go to **Settings** and ensure the **Timezone** is set to `America/Asuncion` or `Browser Local`. This prevents "phantom data" where logs appear to arrive three hours in the future or past.