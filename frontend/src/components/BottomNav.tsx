"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const NAV_ITEMS = [
  { href: "/dashboard",    label: "Dashboard",    icon: "📊" },
  { href: "/upload",       label: "Upload",       icon: "📤" },
  { href: "/transactions", label: "Transactions", icon: "📋" },
];

export function BottomNav() {
  const pathname = usePathname();

  return (
    <nav className="md:hidden fixed bottom-0 left-0 right-0 z-50 bg-white border-t border-gray-200 flex">
      {NAV_ITEMS.map(({ href, label, icon }) => {
        const active = pathname.startsWith(href);
        return (
          <Link
            key={href}
            href={href}
            className={`flex-1 flex flex-col items-center gap-0.5 py-3 text-xs font-medium transition-colors ${
              active ? "text-blue-700" : "text-gray-500"
            }`}
          >
            <span className="text-lg">{icon}</span>
            {label}
          </Link>
        );
      })}
    </nav>
  );
}
