"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "@/lib/api";
import { SummaryCard } from "@/components/SummaryCard";
import { SpendingPieChart } from "@/components/charts/SpendingPieChart";
import { MonthlyBarChart } from "@/components/charts/MonthlyBarChart";
import { MonthNavigator } from "@/components/MonthNavigator";

export default function DashboardPage() {
  const now = new Date();
  const [year, setYear]   = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);

  const { data, isLoading, isError } = useQuery({
    queryKey: ["dashboard", year, month],
    queryFn: () => dashboardApi.get(year, month),
  });

  function handleMonthChange(y: number, m: number) {
    setYear(y);
    setMonth(m);
  }

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
    <div className="p-6 max-w-5xl mx-auto space-y-6">
      {/* Header + month navigator */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <MonthNavigator year={year} month={month} onChange={handleMonthChange} />
        <h1 className="text-sm font-medium text-gray-400">Dashboard</h1>
      </div>

      {/* Summary cards — month scoped */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <SummaryCard label="Income"      value={data.totalIncome}   icon="💰" variant="income" />
        <SummaryCard label="Expenses"    value={data.totalExpenses} icon="💸" variant="expense" />
        <SummaryCard label="Net Balance" value={data.netBalance}    icon="📈" variant={data.netBalance >= 0 ? "income" : "expense"} />
      </div>

      {/* Pie chart — month scoped */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h2 className="text-base font-semibold text-gray-700 mb-4">Spending by Category</h2>
        <SpendingPieChart data={data.spendingByCategory ?? []} />
      </div>

      {/* Bar chart — full year overview */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h2 className="text-base font-semibold text-gray-700 mb-1">{year} Overview</h2>
        <p className="text-xs text-gray-400 mb-4">All months of the year</p>
        <MonthlyBarChart data={data.monthlyTrend ?? []} activeMonth={month} />
      </div>
    </div>
  );
}
