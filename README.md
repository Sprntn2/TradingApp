# Real-Time Trading Application

A real-time currency trading application built with **ASP.NET Core MVC** using a **3-Tier Architecture**, in-memory caching, dual SQL Server persistence, and live UI updates via **SignalR**.

---

## Key Features

- Real-time FX price simulation (±1% fluctuation every 2 seconds)
- In-memory cache (`TradingCache`) with **Write-Back** / **Write-Through** persistence
- Live table updates without page refresh (`SignalR` + `TradingUI`)
- Encapsulated domain model (`CurrencyPair.UpdatePrice`)
- Schema management via **EF Core Migrations**

---

## Solution Structure

```
RealTimeTradingApp/
├── TradingApp.UILayer/          # Presentation (MVC, SignalR Hub, Layout)
├── TradingApp.BusinessLayer/    # Business logic, cache, simulator
├── TradingApp.DataLayer/        # Models, DbContext, Migrations
├── RealTimeTradingApp.slnx
└── README.md
```

### Layer Dependencies

```
UILayer  →  BusinessLayer  →  DataLayer  →  SQL Server (TradingDB)
```

| Project | Responsibility |
|---------|----------------|
| **DataLayer** | `Currency`, `CurrencyPair`, `TradingDbContext`, Migrations |
| **BusinessLayer** | `TradingCache`, `TradingSimulator`, DTOs, view mapping |
| **UILayer** | `HomeController`, Views, `TradingHub`, `Program.cs` |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- **SQL Server** (Express / LocalDB / Developer Edition)
- Visual Studio 2022, VS Code, or Rider (optional)

---

## Database Setup

Update the connection string in `TradingApp.UILayer/appsettings.json` to match your SQL Server instance:

```json
"ConnectionStrings": {
  "TradingDB": "Server=localhost\\SQLEXPRESS;Database=TradingDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

| SQL Instance | Example `Server` value |
|--------------|------------------------|
| SQL Express | `localhost\\SQLEXPRESS` |
| Default instance | `localhost` |
| LocalDB | `(localdb)\\MSSQLLocalDB` |

On startup, the application runs `Database.MigrateAsync()` and automatically creates or updates `TradingDB`.

### Migration Commands (Optional)

```powershell
dotnet ef migrations add <MigrationName> `
  --project TradingApp.DataLayer `
  --startup-project TradingApp.UILayer

dotnet ef database update `
  --project TradingApp.DataLayer `
  --startup-project TradingApp.UILayer
```

---

## Running the Application

```powershell
cd RealTimeTradingApp
dotnet run --project TradingApp.UILayer
```

Open the URL shown in the terminal (e.g. `https://localhost:7039`) in your browser.

---

## Runtime Architecture

### Background Services

| Service | Role |
|---------|------|
| `TradingCache` (`IHostedService`) | Loads data into RAM, syncs every 5 minutes, exposes `GetPairsForView()` |
| `TradingSimulator` (`BackgroundService`) | Updates in-memory prices every 2 seconds |

### Persistence Strategy

| Field | Strategy | Frequency |
|-------|----------|-----------|
| `CurrentValue` | Write-Back | SQL every 5 minutes |
| `MinValue` / `MaxValue` | Write-Through | Immediate SQL on new record highs/lows |

### SignalR

- Hub endpoint: `/hubs/trading`
- Event: `ReceivePriceUpdate`
- Payload: `{ pairId, currentValue, minValue, maxValue }`
- Client: `wwwroot/js/trading-ui.js` → `TradingUI.updatePair()`

---

## Key Folder Structure

```
TradingApp.DataLayer/
  Models/              Currency.cs, CurrencyPair.cs
  Migrations/          EF Core migrations
  TradingDbContext.cs

TradingApp.BusinessLayer/
  DTOs/                CurrencyPairDto.cs
  Mapping/             CurrencyPairDtoMapper.cs
  Services/            TradingCache, TradingSimulator, ITradingCache

TradingApp.UILayer/
  Controllers/         HomeController.cs
  Hubs/                TradingHub.cs
  Services/            SignalRPriceUpdateNotifier.cs
  Views/               Index.cshtml, Shared/_Layout.cshtml
  wwwroot/js/          trading-ui.js
  Program.cs
```

---

## Manual UI Update Test

In the browser console (F12):

```javascript
TradingUI.updatePair(1, { pairId: 1, currentValue: 3.66, minValue: 3.60, maxValue: 3.70 });
```

---

## Troubleshooting

**SQL Server connection error**  
Ensure SQL Server is running and the instance name in the connection string is correct (`SQLEXPRESS` vs `localhost`).

**`signalR is not defined`**  
Verify that `signalr.min.js` loads from `wwwroot/lib/microsoft-signalr/` (Network tab → status 200).

**Empty table on first launch**  
Wait for `TradingCache.StartAsync` to finish loading, or confirm migrations ran successfully.

---

## License

Educational / development project. Update the license as needed.
