"use client";

import { useState } from "react";
import { PieChart, Pie, Cell, ResponsiveContainer } from "recharts";
import type { CategoryBreakdown } from "@/types";

interface Props {
  data: CategoryBreakdown[];
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("it-IT", { style: "currency", currency: "EUR" }).format(value);
}

export function SpendingPieChart({ data }: Props) {
  const [activeIndex, setActiveIndex] = useState<number | null>(null);

  if (!data || data.length === 0) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-400 text-sm">
        No expense data
      </div>
    );
  }

  const active = activeIndex !== null ? data[activeIndex] : null;
  const total = data.reduce((s, d) => s + d.amount, 0);

  return (
    <div className="space-y-3">
      {/* Fixed info panel at top */}
      <div className="h-12 flex items-center justify-center">
        {active ? (
          <div className="flex items-center gap-3">
            <span
              className="inline-block w-3 h-3 rounded-full flex-shrink-0"
              style={{ backgroundColor: active.categoryColor ?? "#6b7280" }}
            />
            <span className="font-semibold text-gray-800 text-sm">{active.categoryName}</span>
            <span className="text-gray-500 text-sm">{formatCurrency(active.amount)}</span>
            <span className="text-gray-400 text-xs">({active.percentage.toFixed(1)}%)</span>
          </div>
        ) : (
          <div className="text-center">
            <p className="text-xs text-gray-400">Total expenses</p>
            <p className="font-semibold text-gray-700 text-sm">{formatCurrency(total)}</p>
          </div>
        )}
      </div>

      <ResponsiveContainer width="100%" height={200}>
        <PieChart>
          <Pie
            data={data}
            dataKey="amount"
            nameKey="categoryName"
            cx="50%"
            cy="50%"
            outerRadius={85}
            innerRadius={45}
            paddingAngle={2}
            onMouseEnter={(_, index) => setActiveIndex(index)}
            onMouseLeave={() => setActiveIndex(null)}
          >
            {data.map((entry, index) => (
              <Cell
                key={index}
                fill={entry.categoryColor ?? "#6b7280"}
                opacity={activeIndex === null || activeIndex === index ? 1 : 0.4}
                stroke={activeIndex === index ? "#fff" : "none"}
                strokeWidth={activeIndex === index ? 2 : 0}
              />
            ))}
          </Pie>
        </PieChart>
      </ResponsiveContainer>

      {/* Legend */}
      <ul className="flex flex-wrap justify-center gap-x-4 gap-y-1">
        {data.map((entry, i) => (
          <li
            key={i}
            className="flex items-center gap-1.5 text-xs text-gray-600 cursor-pointer"
            onMouseEnter={() => setActiveIndex(i)}
            onMouseLeave={() => setActiveIndex(null)}
          >
            <span
              className="inline-block w-2.5 h-2.5 rounded-full flex-shrink-0"
              style={{ backgroundColor: entry.categoryColor ?? "#6b7280" }}
            />
            {entry.categoryName}
          </li>
        ))}
      </ul>
    </div>
  );
}
