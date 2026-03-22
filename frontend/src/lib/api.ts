import type {
  Category,
  DashboardData,
  ImportResult,
  Transaction,
  TransactionFilters,
} from "@/types";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

// ─── Core fetch wrapper ───────────────────────────────────────────────────────

async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { "Content-Type": "application/json", ...init?.headers },
    ...init,
  });

  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`API ${res.status}: ${text}`);
  }

  // 204 No Content
  if (res.status === 204) return undefined as T;

  return res.json() as Promise<T>;
}

// ─── Categories ──────────────────────────────────────────────────────────────

export const categoriesApi = {
  getAll: () => apiFetch<Category[]>("/api/categories"),
};

// ─── Dashboard ───────────────────────────────────────────────────────────────

export const dashboardApi = {
  get: () => apiFetch<DashboardData>("/api/dashboard"),
};

// ─── Transactions ─────────────────────────────────────────────────────────────

export const transactionsApi = {
  getAll: (filters: TransactionFilters = {}) => {
    const params = new URLSearchParams();
    if (filters.page)       params.set("page",       String(filters.page));
    if (filters.pageSize)   params.set("pageSize",   String(filters.pageSize));
    if (filters.categoryId) params.set("categoryId", filters.categoryId);
    if (filters.type)       params.set("type",        filters.type);
    if (filters.from)       params.set("from",        filters.from);
    if (filters.to)         params.set("to",          filters.to);
    const qs = params.toString();
    return apiFetch<Transaction[]>(`/api/transactions${qs ? `?${qs}` : ""}`);
  },

  updateCategory: (id: string, categoryId: string | null) =>
    apiFetch<void>(`/api/transactions/${id}`, {
      method: "PATCH",
      body: JSON.stringify({ categoryId }),
    }),

  delete: (id: string) =>
    apiFetch<void>(`/api/transactions/${id}`, { method: "DELETE" }),
};

// ─── Import ──────────────────────────────────────────────────────────────────

export const importApi = {
  upload: async (file: File): Promise<ImportResult> => {
    const form = new FormData();
    form.append("file", file);

    const res = await fetch(`${BASE_URL}/api/import`, {
      method: "POST",
      body: form,
    });

    if (!res.ok) {
      const text = await res.text().catch(() => res.statusText);
      throw new Error(`API ${res.status}: ${text}`);
    }

    return res.json() as Promise<ImportResult>;
  },
};
