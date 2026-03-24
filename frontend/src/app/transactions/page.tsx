"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { transactionsApi, categoriesApi } from "@/lib/api";
import { CategoryBadge } from "@/components/CategoryBadge";
import { AddTransactionModal } from "@/components/AddTransactionModal";
import type { Transaction } from "@/types";

function formatCurrency(value: number) {
  return new Intl.NumberFormat("it-IT", { style: "currency", currency: "EUR" }).format(value);
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString("it-IT", { day: "2-digit", month: "short", year: "numeric" });
}

function TransactionRow({
  tx,
  onCategoryChange,
  onDelete,
}: {
  tx: Transaction;
  onCategoryChange: (id: string, categoryId: string | null) => void;
  onDelete: (id: string) => void;
}) {
  const { data: categories = [] } = useQuery({
    queryKey: ["categories"],
    queryFn: categoriesApi.getAll,
    staleTime: Infinity,
  });

  return (
    <tr className="border-b border-gray-100 hover:bg-gray-50 transition-colors group">
      <td className="py-3 px-4 text-sm text-gray-500 whitespace-nowrap">{formatDate(tx.date)}</td>
      <td className="py-3 px-4 text-sm text-gray-900 max-w-xs truncate">
        {tx.description}
        {tx.isManuallyCreated && (
          <span className="ml-1.5 text-xs text-blue-400 font-medium">manual</span>
        )}
      </td>
      <td className="py-3 px-4 text-sm font-semibold text-right whitespace-nowrap">
        <span className={tx.type === "Income" ? "text-green-600" : "text-red-500"}>
          {tx.type === "Income" ? "+" : "-"}{formatCurrency(Math.abs(tx.amount))}
        </span>
      </td>
      <td className="py-3 px-4">
        <div className="flex items-center gap-2">
          <select
            value={tx.categoryId ?? ""}
            onChange={(e) => onCategoryChange(tx.id, e.target.value || null)}
            className="text-xs rounded-lg border border-gray-200 bg-white px-2 py-1 focus:outline-none focus:ring-2 focus:ring-blue-300"
          >
            <option value="">— Uncategorized —</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.icon ? `${c.icon} ` : ""}{c.name}
              </option>
            ))}
          </select>
          {tx.categoryName && tx.categoryColor && (
            <CategoryBadge name={tx.categoryName} color={tx.categoryColor} />
          )}
        </div>
      </td>
      <td className="py-3 px-4 text-right">
        <button
          onClick={() => onDelete(tx.id)}
          className="opacity-0 group-hover:opacity-100 w-7 h-7 flex items-center justify-center rounded-full hover:bg-red-50 text-gray-300 hover:text-red-400 transition-all text-sm"
          aria-label="Delete transaction"
        >
          ✕
        </button>
      </td>
    </tr>
  );
}

export default function TransactionsPage() {
  const queryClient = useQueryClient();
  const [showModal, setShowModal] = useState(false);

  const { data: transactions = [], isLoading, isError } = useQuery({
    queryKey: ["transactions"],
    queryFn: () => transactionsApi.getAll({ pageSize: 500 }),
  });

  const { mutate: updateCategory } = useMutation({
    mutationFn: ({ id, categoryId }: { id: string; categoryId: string | null }) =>
      transactionsApi.updateCategory(id, categoryId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
  });

  const { mutate: deleteTransaction } = useMutation({
    mutationFn: (id: string) => transactionsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
  });

  const { mutate: createTransaction, isPending: isCreating } = useMutation({
    mutationFn: transactionsApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      setShowModal(false);
    },
  });

  if (isLoading) {
    return <div className="flex items-center justify-center h-64 text-gray-400">Loading…</div>;
  }

  if (isError) {
    return <div className="flex items-center justify-center h-64 text-red-500">Failed to load transactions.</div>;
  }

  return (
    <div className="p-6 max-w-5xl mx-auto space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Transactions</h1>
          <p className="text-sm text-gray-400 mt-0.5">{transactions.length} records</p>
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-2 px-4 py-2 bg-blue-500 hover:bg-blue-600 text-white text-sm font-medium rounded-xl transition-colors shadow-sm"
        >
          + Add
        </button>
      </div>

      {transactions.length === 0 ? (
        <div className="flex flex-col items-center justify-center h-64 text-gray-400 gap-2">
          <span className="text-4xl">📋</span>
          <p>No transactions yet. Upload a bank statement or add one manually.</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50">
                  <th className="py-3 px-4 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Date</th>
                  <th className="py-3 px-4 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Description</th>
                  <th className="py-3 px-4 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Amount</th>
                  <th className="py-3 px-4 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Category</th>
                  <th className="py-3 px-4 w-10" />
                </tr>
              </thead>
              <tbody>
                {transactions.map((tx) => (
                  <TransactionRow
                    key={tx.id}
                    tx={tx}
                    onCategoryChange={(id, categoryId) => updateCategory({ id, categoryId })}
                    onDelete={(id) => deleteTransaction(id)}
                  />
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {showModal && (
        <AddTransactionModal
          onClose={() => setShowModal(false)}
          onSave={(data) => createTransaction(data)}
          isLoading={isCreating}
        />
      )}
    </div>
  );
}
