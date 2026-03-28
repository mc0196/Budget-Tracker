# Budget Tracker — Progress & Task List

Legend: ✅ Done · 🔲 To Do · ▶️ START HERE (next session)

---

## ▶️ NEXT SESSION — Start Here

### Start backend
```bash
source ~/.zshrc
cd /Users/matteocamero/Documents/SourcePersonal/Budget-Tracker/backend
dotnet run --project BudgetTracker.API
# API: http://localhost:5050
```

### Start frontend
```bash
cd /Users/matteocamero/Documents/SourcePersonal/Budget-Tracker/frontend
npm run dev
# App: http://localhost:3000
```

### Active branches
- `feat/frontend` — frontend changes (open PR → main)
- `feat/backend/transactions-v2` — backend improvements (open PR → main)

### Next tasks
1. Categories management UI (keyword rules, bulk re-categorize)
2. Investments section

---

## Phase 1 — Backend Foundation ✅ COMPLETE

### Setup ✅
- [x] Create .NET 10 solution with 5 projects
- [x] Wire Clean Architecture project references
- [x] Add NuGet packages (EF Core, CsvHelper, ExcelDataReader, xUnit, FluentAssertions, Moq)
- [x] `global.json` pinned to .NET 10 SDK

### Domain Layer ✅
- [x] `Account`, `Category`, `Transaction`, `CategorizationRule`, `ImportLog` entities
- [x] `TransactionType` enum
- [x] `DomainException`

### Application Layer ✅
- [x] Repository interfaces (`ITransactionRepository`, `ICategoryRepository`, `IImportLogRepository`, `ICategorizationRuleRepository`, `IFileParser`)
- [x] DTOs (`TransactionDto`, `DashboardDto`, `CategoryDto`, `ImportResultDto`, `ParsedTransactionRow`)
- [x] Services (`TransactionImportService`, `CategorizationService`, `TransactionService`, `DashboardService`)

### Infrastructure Layer ✅
- [x] `AppDbContext` — EF Core + seed data (10 categories, 10 keyword rules)
- [x] Repositories (`TransactionRepository`, `CategoryRepository`, `ImportLogRepository`, `CategorizationRuleRepository`)
- [x] `CsvFileParser` — signed amounts, debit/credit columns, locale-aware date/decimal
- [x] `ExcelFileParser` — XLS + XLSX, Excel serial dates
- [x] `ParserHelpers` — shared date + amount parsing (stackalloc, no string allocations)
- [x] `DependencyInjection.cs`

### API Layer ✅
- [x] `ImportController` — `POST /api/import`
- [x] `TransactionsController` — `GET/PATCH/DELETE /api/transactions`
- [x] `DashboardController` — `GET /api/dashboard`
- [x] `CategoriesController` — `GET/POST /api/categories`
- [x] `ExceptionHandlingMiddleware` — RFC 7807 error format
- [x] `Program.cs`, `appsettings.json`

---

## Phase 2 — Database Migration ✅ COMPLETE

- [x] `dotnet ef migrations add InitialCreate`
- [x] API starts, DB created, seed data inserted
- [x] `GET /api/categories` returns 10 seeded categories

---

## Phase 3 — Tests ✅ COMPLETE — 59/59 passing

### Unit Tests ✅
- [x] `Transaction` entity — zero/negative amount → `DomainException`
- [x] `Transaction` entity — empty description → `ArgumentException`
- [x] `Transaction` entity — whitespace trimming, category assignment
- [x] `CategorizationRule.Matches()` — case-insensitive, deactivate/activate
- [x] `CsvFileParser` — standard format, debit/credit columns, European decimal, bad dates, empty file
- [x] `ParserHelpers.ParseAmount()` — all currency symbols, US/EU formats, edge cases
- [x] `ParserHelpers.TryParseDate()` — all supported formats, Excel serial, bad input
- [x] `CategorizationService` — matching, no match, priority order, no rules
- [x] `DashboardService` — totals, empty data, monthly grouping, percentages

### Integration Tests ✅
- [x] `POST /api/import` — 200 with row count
- [x] `POST /api/import` — 409 on duplicate file
- [x] `POST /api/import` — 415 for unsupported format
- [x] `POST /api/import` — 400 for missing file

---

## Phase 4 — Frontend (Next.js 15 PWA) ✅ COMPLETE

### Setup ✅
- [x] Next.js 15 + TypeScript + Tailwind CSS v4
- [x] Recharts, @tanstack/react-query
- [x] API client (`lib/api.ts`) pointing to backend on :5050
- [x] Global layout — desktop sidebar + mobile bottom nav

### Pages ✅
- [x] `/dashboard` — month navigation, summary cards, spending pie chart, monthly bar chart
- [x] `/upload` — drag-and-drop CSV/Excel import
- [x] `/transactions` — list with inline category editor, add manual transaction, delete

### Components ✅
- [x] `SummaryCard`, `SpendingPieChart` (donut), `MonthlyBarChart`
- [x] `MonthNavigator` — prev/next month with Today button
- [x] `AddTransactionModal` — form to add transactions manually
- [x] `CategoryBadge`, `FileDropzone`

---

## Phase 5 — Backend Improvements ✅ COMPLETE

- [x] Fix Excel parser for Intesa Sanpaolo format (20+ metadata rows, DateTime cells)
- [x] `GET /api/dashboard?year=&month=` — month-scoped cards + pie, year bar chart
- [x] `POST /api/transactions` — manual transaction entry
- [x] Investments + Savings categories added to seed data
- [x] Auto-categorization rules for degiro, fineco, directa, trading

---

## Phase 6 — In Progress

- [x] ✅ Transactions page: month filter (MonthNavigator + GET ?year=&month=)
- [x] ✅ POST /api/transactions — create manual transaction (was missing from backend)
- [x] ✅ Dashboard bug fix: cards+pie now month-scoped, bar chart year-scoped
- [ ] 🔲 Categories management UI (add/edit keyword rules, bulk re-categorize)
- [ ] 🔲 Investments section (view total invested, breakdown by broker)
- [ ] 🔲 Export transactions to CSV
- [ ] 🔲 Pagination on transactions list
- [ ] 🔲 PWA: service worker, offline fallback, iOS meta tags
- [ ] 🔲 Docker Compose (API + frontend)
- [ ] 🔲 PostgreSQL guide for production

---

## Build Status

| Project | Status |
|---|---|
| `BudgetTracker.Domain` | ✅ Builds — 0 errors |
| `BudgetTracker.Application` | ✅ Builds — 0 errors |
| `BudgetTracker.Infrastructure` | ✅ Builds — 0 errors |
| `BudgetTracker.API` | ✅ Builds — 0 errors |
| `BudgetTracker.Tests` | ✅ **59/59 tests passing** |
| Frontend | ✅ Running on :3000 |

## How to Run Backend

```bash
export DOTNET_ROOT=/usr/local/share/dotnet
export PATH="/usr/local/share/dotnet:$PATH"
cd backend
dotnet run --project BudgetTracker.API
# API: http://localhost:5050
# Swagger: http://localhost:5050/swagger

# Run tests
dotnet test
```

Last updated: 2026-03-28
