interface SummaryCardProps {
  label: string;
  value: number;
  icon: string;
  variant?: "default" | "income" | "expense";
}

const variantStyles = {
  default: "bg-white border-gray-200 text-gray-900",
  income:  "bg-green-50 border-green-200 text-green-700",
  expense: "bg-red-50 border-red-200 text-red-700",
};

function formatCurrency(value: number) {
  return new Intl.NumberFormat("it-IT", {
    style: "currency",
    currency: "EUR",
  }).format(value);
}

export function SummaryCard({
  label,
  value,
  icon,
  variant = "default",
}: SummaryCardProps) {
  return (
    <div
      className={`rounded-xl border p-5 flex items-center gap-4 ${variantStyles[variant]}`}
    >
      <span className="text-3xl">{icon}</span>
      <div>
        <p className="text-sm font-medium opacity-70">{label}</p>
        <p className="text-2xl font-bold">{formatCurrency(value)}</p>
      </div>
    </div>
  );
}
