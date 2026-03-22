"use client";

import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from "recharts";
import type { CategoryBreakdown } from "@/types";

interface Props {
  data: CategoryBreakdown[];
}

export function SpendingPieChart({ data }: Props) {
  if (data.length === 0) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-400 text-sm">
        No expense data
      </div>
    );
  }

  return (
    <ResponsiveContainer width="100%" height={280}>
      <PieChart>
        <Pie
          data={data}
          dataKey="total"
          nameKey="categoryName"
          cx="50%"
          cy="50%"
          outerRadius={90}
          label={({ name, percent }) =>
            `${name} ${((percent ?? 0) * 100).toFixed(0)}%`
          }
          labelLine={false}
        >
          {data.map((entry) => (
            <Cell key={entry.categoryId} fill={entry.color} />
          ))}
        </Pie>
        <Tooltip
          formatter={(value) =>
            new Intl.NumberFormat("it-IT", {
              style: "currency",
              currency: "EUR",
            }).format(Number(value))
          }
        />
        <Legend />
      </PieChart>
    </ResponsiveContainer>
  );
}
