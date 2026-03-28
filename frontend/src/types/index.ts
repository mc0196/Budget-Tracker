// ─── Enums ───────────────────────────────────────────────────────────────────

export type TransactionType = "Income" | "Expense";

// ─── Domain types ─────────────────────────────────────────────────────────────

export interface Category {
  id: string;
  name: string;
  type: TransactionType;
  color: string;
  icon: string | null;
}

export interface Transaction {
  id: string;
  date: string; // ISO date string
  description: string;
  amount: number;
  type: TransactionType;
  categoryId: string | null;
  categoryName: string | null;
  categoryColor: string | null;
}

// ─── Dashboard ───────────────────────────────────────────────────────────────

export interface CategoryBreakdown {
  categoryName: string;
  categoryColor: string | null;
  amount: number;
  percentage: number;
}

export interface MonthlyTotal {
  year: number;
  month: number;
  monthLabel: string;
  income: number;
  expenses: number;
  net: number;
}

export interface DashboardData {
  totalIncome: number;
  totalExpenses: number;
  netBalance: number;
  spendingByCategory: CategoryBreakdown[];
  monthlyTrend: MonthlyTotal[];
}

// ─── Import ──────────────────────────────────────────────────────────────────

export interface ImportResult {
  rowsImported: number;
  fileName: string;
}

// ─── Filters ─────────────────────────────────────────────────────────────────

export interface TransactionFilters {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  type?: TransactionType;
  from?: string;
  to?: string;
  year?: number;
  month?: number;
}
