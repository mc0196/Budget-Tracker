# Budget Tracker — Progress & Task List

Legend: ✅ Done · 🔲 To Do · ▶️ START HERE (next session)

---

## ▶️ NEXT SESSION — Start Here

### Step 1 — Git setup (5 min)
```bash
cd /Users/matteocamero/Documents/SourcePersonal/Budget-Tracker
git init
# Claude will create the .gitignore
git add .
git commit -m "feat: initial backend — Clean Architecture, .NET 10, 59 tests passing"

# Push to GitHub (create the repo on github.com first, then):
git remote add origin https://github.com/YOUR_USERNAME/budget-tracker.git
git branch -M main
git push -u origin main

# Create frontend branch
git checkout -b feat/frontend
```

### Step 2 — Frontend scaffold (10 min)
```bash
cd /Users/matteocamero/Documents/SourcePersonal/Budget-Tracker
npx create-next-app@latest frontend --typescript --tailwind --app --src-dir
cd frontend
npm install recharts @tanstack/react-query next-pwa
```

### Step 3 — Continue with Claude
Open Claude Code and say: **"continue with the frontend"**
Claude has full context and will pick up from Step 3 in Phase 4 below.

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

## Phase 4 — Frontend (Next.js 15 PWA) 🔲 NOT STARTED

### Setup
- [ ] 🔲 `npx create-next-app@latest frontend --typescript --tailwind --app --src-dir`
- [ ] 🔲 Install: `recharts`, `@tanstack/react-query`, `next-pwa`
- [ ] 🔲 Configure `next.config.js` with PWA plugin
- [ ] 🔲 Set up API client (`lib/api.ts`)
- [ ] 🔲 Global layout — desktop sidebar + mobile bottom nav bar

### Pages
- [ ] 🔲 `/dashboard` — summary cards + spending pie chart + monthly bar chart
- [ ] 🔲 `/upload` — drag-and-drop file upload with progress + result feedback
- [ ] 🔲 `/transactions` — list with inline category editor

### Components
- [ ] 🔲 `SummaryCard` — income / expenses / net balance
- [ ] 🔲 `SpendingPieChart` — Recharts pie chart by category
- [ ] 🔲 `MonthlyBarChart` — Recharts bar chart, income vs expenses
- [ ] 🔲 `TransactionRow` — row with editable category dropdown
- [ ] 🔲 `FileDropzone` — drag-and-drop CSV/Excel upload
- [ ] 🔲 `CategoryBadge` — colored pill with icon

---

## Phase 5 — PWA 🔲 NOT STARTED

- [ ] 🔲 `public/manifest.json`
- [ ] 🔲 Service worker via `next-pwa`
- [ ] 🔲 Offline fallback page
- [ ] 🔲 iOS meta tags
- [ ] 🔲 Test "Add to Home Screen" on iOS + Android

---

## Phase 6 — Polish & Future

- [ ] 🔲 Git + GitHub setup
- [ ] 🔲 `.gitignore` for .NET + Node
- [ ] 🔲 Export transactions to CSV
- [ ] 🔲 Categorization rules management UI
- [ ] 🔲 Pagination on transactions list
- [ ] 🔲 Docker Compose (API + frontend)
- [ ] 🔲 PostgreSQL guide for production
- [ ] 🔲 Open Banking / PSD2 integration

---

## Build Status

| Project | Status |
|---|---|
| `BudgetTracker.Domain` | ✅ Builds — 0 errors |
| `BudgetTracker.Application` | ✅ Builds — 0 errors |
| `BudgetTracker.Infrastructure` | ✅ Builds — 0 errors |
| `BudgetTracker.API` | ✅ Builds — 0 errors |
| `BudgetTracker.Tests` | ✅ **59/59 tests passing** |
| Frontend | 🔲 Not started |

## How to Run Backend

```bash
export DOTNET_ROOT=/usr/local/share/dotnet
export PATH="/usr/local/share/dotnet:$PATH"
cd backend
dotnet run --project BudgetTracker.API
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger

# Run tests
dotnet test
```

Last updated: 2026-03-22
