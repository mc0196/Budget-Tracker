# Budget Tracker

A personal finance tracking web application to manage income and expenses with minimal manual input.
Installable on mobile as a Progressive Web App (PWA).

---

## Tech Stack

### Backend
| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 10 (SQLite dev / PostgreSQL prod) |
| CSV Parsing | CsvHelper |
| Excel Parsing | ExcelDataReader |
| Testing | xUnit + FluentAssertions + Moq |

### Frontend
| Layer | Technology |
|---|---|
| Framework | Next.js 15 (App Router) |
| Language | TypeScript |
| Styling | Tailwind CSS |
| Charts | Recharts |
| PWA | next-pwa (service worker + manifest) |
| HTTP Client | fetch / React Query |

---

## Architecture

Clean Architecture — inner layers never depend on outer layers.

```
┌──────────────────────────────────────────┐
│               API Layer                  │  ← Controllers, Middleware, Program.cs
├──────────────────────────────────────────┤
│           Application Layer              │  ← Services, Use Cases, DTOs, Interfaces
├──────────────────────────────────────────┤
│             Domain Layer                 │  ← Entities, Enums, Business Rules
├──────────────────────────────────────────┤
│          Infrastructure Layer            │  ← EF Core, Repositories, File Parsers
└──────────────────────────────────────────┘
```

**Dependency rule:** Domain ← Application ← Infrastructure → API

---

## Project Structure

```
Budget-Tracker/
├── backend/
│   ├── BudgetTracker.Domain/           Pure domain: entities, enums, exceptions
│   ├── BudgetTracker.Application/      Use cases, interfaces (ports), DTOs
│   ├── BudgetTracker.Infrastructure/   EF Core, repositories, file parsers
│   ├── BudgetTracker.API/              HTTP layer: controllers, middleware
│   └── BudgetTracker.Tests/            Unit + integration tests
│
├── frontend/                           Next.js 15 PWA
│
├── global.json                         Pins SDK to .NET 10
└── PROGRESS.md                         Task tracking
```

---

## Core Features

- **Upload** CSV or Excel bank statements
- **Auto-parse** transactions (date, description, amount, income/expense)
- **Auto-categorize** via keyword rules (e.g. "netflix" → Entertainment)
- **Dashboard** with total income, expenses, net balance, spending by category, monthly trends
- **Edit** transaction categories and descriptions manually
- **Idempotent imports** — SHA-256 file hash prevents double-importing the same file
- **PWA** — installable on iPhone/Android, basic offline support

---

## Key Design Decisions

| Decision | Reason |
|---|---|
| SQLite for dev | Zero configuration, file-based, swap to PostgreSQL in prod via one config line |
| Positive amounts only | `Transaction.Amount` is always positive; `TransactionType` enum encodes direction. Avoids signed-amount confusion in queries. |
| SHA-256 import hash | Re-uploading the same bank statement is silently rejected instead of creating duplicates |
| Keyword rules for categorization | Works with zero dependencies; can be extended with AI later without changing the rest |
| CORS origins in appsettings.json | Different per environment (dev/staging/prod) without code changes |
| `global.json` pinning .NET 10 | Two dotnet installations on this machine (x64 .NET 9, arm64 .NET 10); pin ensures the right SDK is always used |

---

## Running Locally

### Backend

```bash
# First time only — set PATH to .NET 10 (arm64)
export DOTNET_ROOT=/usr/local/share/dotnet
export PATH="/usr/local/share/dotnet:$PATH"

cd backend
dotnet run --project BudgetTracker.API

# API:     https://localhost:7xxx
# Swagger: https://localhost:7xxx/swagger
```

### Frontend (once scaffolded)

```bash
cd frontend
npm run dev
# App: http://localhost:3000
```

### Run Tests

```bash
cd backend
dotnet test
```

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/import` | Upload CSV or Excel bank statement |
| `GET` | `/api/transactions` | List all transactions |
| `GET` | `/api/transactions/{id}` | Get single transaction |
| `PATCH` | `/api/transactions/{id}` | Update category / description / amount |
| `DELETE` | `/api/transactions/{id}` | Delete a transaction |
| `GET` | `/api/dashboard` | Dashboard aggregations (income, expenses, trends) |
| `GET` | `/api/categories` | List all categories |
| `POST` | `/api/categories` | Create a new category |

---

## Database Schema (simplified)

```
accounts          — bank account source
categories        — income/expense labels (seeded with 10 defaults)
transactions      — core entity (date, description, amount, type, category)
categorization_rules — keyword → category mappings (auto-classification)
import_logs       — file hash + row counts (idempotency guard)
```
