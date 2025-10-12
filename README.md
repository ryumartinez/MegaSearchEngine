# MegaSearchEngine – Branch Comparison & Architecture Notes  
**main → IDesignRefactor**

## Overview
The `IDesignRefactor` branch adopts **Juval Lowy’s IDesign architecture** to enforce a clear separation of concerns:

- **Manager** — orchestrates application use-cases (no I/O).
- **Engine** — pure computation/business logic.
- **DataAccess** — external resources (network, DB, filesystem).
- **API** — service boundary/transport only.
- **Utils** — cross-cutting helpers.

### Core Responsibility Split (the big change)
- **DataAccess** now **fetches** HTML with **Playwright**.
- **Engine** now **parses** HTML **only** with **AngleSharp**.
- **Manager** coordinates the workflow and persistence.

---

## Architectural Roles (IDesign)

### Manager (Use-case orchestration)
- Coordinates fetching, parsing, and persistence across layers.
- Aggregates, **de-duplicates** (by Link, fallback Title), and orders results.
- No direct external I/O; relies on contracts.

### Engine (Computation)
- Parses HTML into structured results (no I/O).
- Site-specific parsers behind `IEcommerceParserEngine`.
- Uses **AngleSharp** to traverse/extract DOM.

### DataAccess (Resources)
- Fetches HTML via **Playwright** behind `IHtmlFetcher`, with **ready CSS selectors** and **timeouts** per site.
- Persists results through `IProductDataAccess` (EF Core; migrations present).
- Centralizes resource concerns (timeouts, waits, navigation).

### API
- Exposes endpoints; composes Managers into HTTP.
- No business logic or persistence logic inside endpoints.

---

## Key Contracts
- **`IHtmlFetcher`** (DataAccess): `GetContentAsync(Uri url, string readySelector, int timeoutMs, CancellationToken ct)`
- **`IEcommerceParserEngine`** (Engine): `ParseSearchHtml(string html, Uri sourceUrl, string siteName)`
- **`IProductDataAccess`** (DataAccess): persistence (`GetAsync`, `AddRangeAsync`, …)

---

## Orchestrated Flow (Manager)

### `SearchAndSaveAsync(string searchText)`
1. **Fetch & Parse per site**
   - Manager calls `IHtmlFetcher.GetContentAsync(...)` with **site URL**, **ready selector**, **timeout**:
     - PuntoFarma → `div.card-producto_cardProducto__Jl8Pw` (≈ 15s)
     - FarmaTotal → `div.product` (≈ 20s)
     - Biggie → `div.card-container` (≈ 15s)
   - Passes HTML to the **keyed** `IEcommerceParserEngine` (`"puntoFarma"`, `"farmaTotal"`, `"biggie"`).
2. **Aggregate & De-dupe**
   - Flattens results; de-dupes by **Link** (fallback: Title) via `ConcurrentDictionary`.
3. **Persist**
   - Maps to `CreateProductAccessRequest` and calls `IProductDataAccess.AddRangeAsync(...)`.

### `SearchAndSaveManyAsync(IEnumerable<string> searchTexts)`
- Normalizes terms (trim, distinct, ignore-empty), repeats `SearchAndSaveAsync` per term, honors `CancellationToken`.

### `GetAsync(GetSearchResultItemRequest request)`
- Reads persisted data via `IProductDataAccess.GetAsync(...)`.
- Returns totals and per-site counts.

---

## 🕒 Scheduler API (Daily Batch)
A **Scheduler API** is introduced to trigger a **daily** batch search that populates the datastore by calling the batch endpoint.

### Endpoints (in `Api.Endpoints.SearchEndpoints`)
- `GET /search`  
  Returns aggregated stored results (optionally takes `searchText` for compatibility).
- `POST /search/run`  
  Triggers **fetch → parse → persist** for a **single** search term (body: string).
- `POST /search/run/batch`  
  Triggers **fetch → parse → persist** for **multiple** search terms (body: `IEnumerable<string>`).

### Daily Job Behavior
- **Once per day**, the scheduler composes a list of search terms and calls:
  - `POST /search/run/batch`
- The API methods are **cancellable** (accept `CancellationToken`) and return `204 No Content` on success.
- Input validation is performed:
  - `run`: requires non-empty `searchText`
  - `run/batch`: requires a non-null collection

### Example requests
```bash
# Single term
curl -X POST http://<host>/search/run \
  -H "Content-Type: application/json" \
  -d '"ibuprofeno"'

# Batch
curl -X POST http://<host>/search/run/batch \
  -H "Content-Type: application/json" \
  -d '["ibuprofeno","paracetamol","vitamina c"]'
