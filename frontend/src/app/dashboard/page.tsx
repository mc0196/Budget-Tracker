"use client";

import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "@/lib/api";
import { SummaryCard } from "@/components/SummaryCard";
import { SpendingPieChart } from "@/components/charts/SpendingPieChart";
import { MonthlyBarChart } from "@/components/charts/MonthlyBarChart";

export default function DashboardPage() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ["dashboard"],
    queryFn: dashboardApi.get,
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-400">
        Loading…
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="flex items-center justify-center h-64 text-red-500">
        Failed to load dashboard data.
      </div>
    );
  }

  return (
    <div className="p-6 max-w-5xl mx-auto space-y-8">
      <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>

      {/* Summary cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <SummaryCard label="Income"      value={data.totalIncome}   icon="💰" variant="income" />
        <SummaryCard label="Expenses"    value={data.totalExpenses} icon="💸" variant="expense" />
        <SummaryCard label="Net Balance" value={data.netBalance}    icon="📈" variant={data.netBalance >= 0 ? "income" : "expense"} />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-base font-semibold text-gray-700 mb-4">Spending by Category</h2>
          <SpendingPieChart data={data.expensesByCategory ?? []} />
        </div>

        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-base font-semibold text-gray-700 mb-4">Monthly Overview</h2>
          <MonthlyBarChart data={data.monthlyTotals ?? []} />
        </div>
      </div>
    </div>
  );
}
