"use client";

import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from "recharts";
import type { MonthlyTotal } from "@/types";

interface Props {
  data: MonthlyTotal[];
  activeMonth?: number; // 1-12, highlights this month's bars
}

const MONTH_NAMES = ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"];

function formatCurrency(value: number) {
  return new Intl.NumberFormat("it-IT", {
    style: "currency",
    currency: "EUR",
    maximumFractionDigits: 0,
  }).format(value);
}

function CustomTooltip({ active, payload, label }: {
  active?: boolean;
  payload?: { name: string; value: number }[];
  label?: string;
}) {
  if (!active || !payload?.length) return null;
  const income   = payload.find((p) => p.name === "Income");
  const expenses = payload.find((p) => p.name === "Expenses");
  const net = (income?.value ?? 0) - (expenses?.value ?? 0);
  return (
    <div className="bg-white border border-gray-200 rounded-lg shadow-lg px-3 py-2 text-sm space-y-1 min-w-[150px]">
      <p className="font-semibold text-gray-800 mb-1">{label}</p>
      {income && (
        <div className="flex justify-between gap-4">
          <span className="text-gray-500">Income</span>
          <span className="font-medium text-green-600">{formatCurrency(income.value)}</span>
        </div>
      )}
      {expenses && (
        <div className="flex justify-between gap-4">
          <span className="text-gray-500">Expenses</span>
          <span className="font-medium text-red-500">{formatCurrency(expenses.value)}</span>
        </div>
      )}
      <div className="flex justify-between gap-4 border-t border-gray-100 pt-1 mt-1">
        <span className="text-gray-500">Balance</span>
        <span className={`font-semibold ${net >= 0 ? "text-green-600" : "text-red-500"}`}>
          {formatCurrency(net)}
        </span>
      </div>
    </div>
  );
}

export function MonthlyBarChart({ data, activeMonth }: Props) {
  if (!data || data.length === 0) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-400 text-sm">
        No monthly data
      </div>
    );
  }

  const chartData = data.map((d) => ({
    name: MONTH_NAMES[d.month - 1],
    Income: d.income,
    Expenses: d.expenses,
    month: d.month,
  }));

  return (
    <div className="space-y-3">
      {/* Legend */}
      <div className="flex items-center gap-4 text-xs text-gray-500">
        <span className="flex items-center gap-1.5">
          <span className="inline-block w-3 h-3 rounded-sm bg-green-400" />
          Income
        </span>
        <span className="flex items-center gap-1.5">
          <span className="inline-block w-3 h-3 rounded-sm bg-red-400" />
          Expenses
        </span>
      </div>

      <ResponsiveContainer width="100%" height={240}>
        <BarChart data={chartData} margin={{ top: 4, right: 4, left: 0, bottom: 0 }} barCategoryGap="30%">
          <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
          <XAxis
            dataKey="name"
            tick={{ fontSize: 11, fill: "#9ca3af" }}
            axisLine={false}
            tickLine={false}
          />
          <YAxis
            tickFormatter={(v) => v >= 1000 ? `€${(v / 1000).toFixed(0)}k` : `€${v}`}
            tick={{ fontSize: 11, fill: "#9ca3af" }}
            axisLine={false}
            tickLine={false}
            width={45}
          />
          <Tooltip content={<CustomTooltip />} cursor={{ fill: "#f9fafb" }} />
          <Bar dataKey="Income" radius={[4, 4, 0, 0]}>
            {chartData.map((entry, index) => (
              <Cell
                key={index}
                fill={entry.month === activeMonth ? "#16a34a" : "#4ade80"}
                opacity={activeMonth && entry.month !== activeMonth ? 0.5 : 1}
              />
            ))}
          </Bar>
          <Bar dataKey="Expenses" radius={[4, 4, 0, 0]}>
            {chartData.map((entry, index) => (
              <Cell
                key={index}
                fill={entry.month === activeMonth ? "#dc2626" : "#f87171"}
                opacity={activeMonth && entry.month !== activeMonth ? 0.5 : 1}
              />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
