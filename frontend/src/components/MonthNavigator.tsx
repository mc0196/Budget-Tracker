"use client";

const MONTHS = ["January","February","March","April","May","June","July","August","September","October","November","December"];

interface Props {
  year: number;
  month: number;
  onChange: (year: number, month: number) => void;
}

export function MonthNavigator({ year, month, onChange }: Props) {
  const now = new Date();
  const isCurrentMonth = year === now.getFullYear() && month === now.getMonth() + 1;

  function prev() {
    if (month === 1) onChange(year - 1, 12);
    else onChange(year, month - 1);
  }

  function next() {
    if (month === 12) onChange(year + 1, 1);
    else onChange(year, month + 1);
  }

  return (
    <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
      <button
        onClick={prev}
        style={{ width: 40, height: 40, borderRadius: 10, border: "1px solid #e5e7eb", background: "#fff", fontSize: 20, cursor: "pointer", color: "#374151" }}
        aria-label="Previous month"
      >
        ‹
      </button>

      <div style={{ minWidth: 160, textAlign: "center" }}>
        <div style={{ fontSize: 18, fontWeight: 700, color: "#111827" }}>{MONTHS[month - 1]}</div>
        <div style={{ fontSize: 12, color: "#9ca3af" }}>{year}</div>
      </div>

      <button
        onClick={next}
        disabled={isCurrentMonth}
        style={{ width: 40, height: 40, borderRadius: 10, border: "1px solid #e5e7eb", background: "#fff", fontSize: 20, cursor: isCurrentMonth ? "not-allowed" : "pointer", color: "#374151", opacity: isCurrentMonth ? 0.3 : 1 }}
        aria-label="Next month"
      >
        ›
      </button>

      {!isCurrentMonth && (
        <button
          onClick={() => onChange(now.getFullYear(), now.getMonth() + 1)}
          style={{ fontSize: 12, color: "#3b82f6", background: "#eff6ff", border: "1px solid #bfdbfe", borderRadius: 8, padding: "4px 10px", cursor: "pointer", fontWeight: 500 }}
        >
          Today
        </button>
      )}
    </div>
  );
}
